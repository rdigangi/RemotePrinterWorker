using log4net;
using Microsoft.Win32;
using RemotePrinterWorker.Handlers;
using RemotePrinterWorker.Helpers;
using RemotePrinterWorker.Models.Enum;
using RemotePrinterWorker.Models.Exceptions;
using RemotePrinterWorker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RemotePrinterWorker
{
    public partial class RemotePrinterWorker : ServiceBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IContainer components;
        private EventLog eventLog;
        public string fraz { get; set; }
        public string printerName { get; set; }
        public string blobLink { get; set; }


        protected override void OnStart(string[] args)
        {
            Pdl pdl = new Pdl();
            try
            {
                /* Settaggio stato servizio: Running */
                ServiceStates.ServiceStatus serviceStatus = default(ServiceStates.ServiceStatus);
                serviceStatus.dwCurrentState = ServiceStates.ServiceState.SERVICE_RUNNING;
                SetServiceStatus(base.ServiceHandle, ref serviceStatus);

                /* Fix Marzo 2024 */
                _logger.Info("Service version: 1.6");
                /* -------------- */

                _logger.Info("Service status: RUNNING");

                /* Validazione configurazione */
                if (!ValidateConfig())
                {
                    /* Stoppo il servizio in caso di validazione non superata */
                    OnStop();
                    return;
                }

                /* Inizializzazione parametri di retry e timer */
                int iteration = 1;
                int maxRetry = int.Parse(ConfigurationManager.AppSettings["MaxRetry"]);
                int retryIntervalSeconds = int.Parse(ConfigurationManager.AppSettings["RetryIntervalSeconds"]);
                int timerSDP = int.Parse(ConfigurationManager.AppSettings["TimerSDP"]);
                int maxRetySDP = int.Parse(ConfigurationManager.AppSettings["MaxRetrySDP"]);

                string text = "Sono stati configurati i seguenti settaggi per i retry per le chiamate: ";
                text += ((retryIntervalSeconds > 0) ? ("sarà effettuato un tentativo ogni " + retryIntervalSeconds + " secondi ") : "nessun retry previsto dai settaggi in configurazione");

                if (retryIntervalSeconds > 0)
                {
                    text += ((maxRetry > 0) ? ("fino ad un massimo di " + maxRetry + " tentativi.") : " senza un limite di tentativi");
                }

                _logger.Info(text);

                #region RECUPERO CONTESTO PDL
                /* Recupero contesto PDL */
                pdl = GetPdlContext();

                if (pdl == null)
                {
                    /* Stoppo il servizio se non valorizzo la PDL */
                    _logger.Error("Errore bloccante durante il recupero del contesto");
                    OnStop();
                    return;
                }

                _logger.Info("Informazioni di contesto della PDL recuperate.");
                _logger.Info("Servizio avviato su frazionario: " + pdl.fraz + " e PDL: " + pdl.pdl);
                #endregion

                #region VERIFICA ABILITAZIONE ODA
                /* Verifico abilitazione della PDL all'ODA */
                EtcdRoot etcdRootPdlEnabled = CheckOdaEnabled(maxRetry, retryIntervalSeconds, pdl);

                /* Se sono uscito dal ciclo e non ho ancora recuperato correttamente il nodo comunico l'errore e stoppo il servizio */
                if (etcdRootPdlEnabled == null || etcdRootPdlEnabled.action == "404" || !Convert.ToBoolean(etcdRootPdlEnabled.node?.value))
                {
                    _logger.Error(("Errore durante la verifica dell'abilitazione." + etcdRootPdlEnabled == null) ? "" : " Pdl non abilitata al servizio.");
                    OnStop();
                    return;
                }
                #endregion

                #region RECUPERO INTERVALLO DI POLLING
                /* Recupero l'intervallo di polling dalla configurazione */
                EtcdRoot etcdRootPollingInterval = GetPollingInterval(maxRetry, retryIntervalSeconds);

                /* Se sono uscito dal ciclo e non ho ancora recuperato correttamente il nodo comunico l'errore e stoppo il servizio */
                if (etcdRootPollingInterval == null || etcdRootPollingInterval.action == "404" || Convert.ToInt32(etcdRootPollingInterval.node?.value) <= 0)
                {
                    _logger.Error("Errore durante il recupero dell'intervallo di polling da ETCD.");
                    OnStop();
                    return;
                }
                #endregion

                #region RECUPERO PRINTER_NAME
                /* Recupero il nome della stampante selezionata in ETCD */
                EtcdRoot etcdRootPrinterName = GetPrinterName(pdl, maxRetry, retryIntervalSeconds);

                /* Se sono uscito dal ciclo e non ho ancora recuperato correttamente il nodo comunico l'errore e stoppo il servizio */
                if (etcdRootPrinterName == null || etcdRootPrinterName.action == "404" || string.IsNullOrEmpty(etcdRootPrinterName.node?.value))
                {
                    _logger.Error("Errore durante il recupero dell'intervallo di polling da ETCD.");
                    OnStop();
                    return;
                }

                printerName = etcdRootPrinterName.node?.value;

                _logger.Info("Stampante configurata su ETCD: " + printerName);
                #endregion

                List<RemotePrinter> list = new List<RemotePrinter>();

                _logger.Debug("Timer di contatto del guscio impostato a: " + timerSDP + " secondi");

                while (true)
                {
                    try
                    {
                        iteration = 1;
                        bool flag = false;

                        while (maxRetySDP == 0 || iteration < maxRetySDP)
                        {
                            try
                            {
                                list = WecomHelper.GetRemotePrinters();
                                if (list == null || list.Count == 0)
                                {
                                    _logger.Debug("Errore durante la verifica delle stampanti remote disponibili.");
                                    continue;
                                }
                                flag = true;
                            }
                            catch (Exception ex2)
                            {
                                _logger.Error(ex2.Message);
                                continue;
                            }
                            finally
                            {
                                if (!flag)
                                {
                                    iteration++;
                                    _logger.Debug("Attendo timer SDP");
                                    Thread.Sleep(timerSDP * 1000);
                                }
                            }
                            break;
                        }

                        if (list == null || list.Count == 0)
                        {
                            continue;
                        }

                        if (!list.Any((RemotePrinter a) => a.printerName == printerName))
                        {
                            printerName = list.First().printerName;
                        }

                        _logger.Info("Stampante selezionata: " + printerName);

                        System.Timers.Timer timer = new System.Timers.Timer();
                        timer.Interval = int.Parse(etcdRootPollingInterval.node.value) * 1000;
                        timer.Elapsed += OnTimer;
                        timer.Start();
                        break;
                    }
                    catch (Exception ex3)
                    {
                        _logger.Error(ex3.Message);
                    }
                }
            }
            catch (Exception ex4)
            {
                _logger.Error("Errore bloccante: " + ex4.Message);
                OnStop();
            }
        }

        private EtcdRoot GetPrinterName(Pdl pdl, int maxRetry, int retryIntervalSeconds)
        {
            string etcdPrinterName = ConfigurationManager.AppSettings["etcdPrinterName"];
            etcdPrinterName = etcdPrinterName.Replace("#fraz_pdl", pdl.fraz + "_" + pdl.pdl);
            EtcdRoot etcdRootPrinterName = null;
            int iteration = 1;

            while (maxRetry == 0 || iteration < maxRetry)
            {
                _logger.Debug("Tentativo di connessione a etcd n." + iteration);
                etcdRootPrinterName = EtcdHelper.GetNodeFromKey(etcdPrinterName);
                if (etcdRootPrinterName == null)
                {
                    /* In caso di nodo non recuperato incremento le iterazioni e passo alla prossima iterazione di loop dopo aver atteso l'intervallo */
                    _logger.Error("Errore durante il recupero dell'intervallo di polling da ETCD.");
                    iteration++;
                    Thread.Sleep(retryIntervalSeconds * 1000);
                    continue;
                }
                /* Se ho il nodo comunico l'avvenuto recupero ed esco dal ciclo */
                _logger.Info("Comunicazione con ETCD per recupero printerName riuscita al tentativo n." + iteration);
                break;
            }

            if (etcdRootPrinterName != null && etcdRootPrinterName.action != "404" && !string.IsNullOrEmpty(etcdRootPrinterName.node?.value))
            {
                _logger.Debug("Etcd PrinterName key: " + etcdPrinterName + " - Etcd value: " + printerName);
            }

            return etcdRootPrinterName;
        }

        private EtcdRoot GetPollingInterval(int maxRetry, int retryIntervalSeconds)
        {
            string key = ConfigurationManager.AppSettings["etcdRemotePrintPollingInterval"];
            EtcdRoot etcdRootPollingInterval = null;
            int iteration = 1;

            while (maxRetry == 0 || iteration < maxRetry)
            {
                _logger.Debug("Tentativo di connessione a etcd n." + iteration);
                etcdRootPollingInterval = EtcdHelper.GetNodeFromKey(key);
                if (etcdRootPollingInterval == null)
                {
                    /* In caso di nodo non recuperato incremento le iterazioni e passo alla prossima iterazione di loop dopo aver atteso l'intervallo */
                    _logger.Error("Errore durante il recupero dell'intervallo di polling da ETCD.");
                    iteration++;
                    Thread.Sleep(retryIntervalSeconds * 1000);
                    continue;
                }
                /* Se ho il nodo comunico l'avvenuto recupero ed esco dal ciclo */
                _logger.Info("Comunicazione con ETCD per recuper polling riuscita al tentativo n." + iteration);
                break;
            }

            return etcdRootPollingInterval;
        }

        private EtcdRoot CheckOdaEnabled(int maxRetry, int retryIntervalSeconds, Pdl pdl)
        {
            int iteration = 1;
            EtcdRoot etcdRootPdlEnabled = null;

            /* Costruisco path ETCD con frazionario e PDL */
            fraz = pdl.fraz;
            string etcdKey = ConfigurationManager.AppSettings["etcdRemotePrintEnabled"];
            etcdKey = etcdKey.Replace("#fraz_pdl", pdl.fraz + "_" + pdl.pdl);

            while (maxRetry == 0 || iteration < maxRetry)
            {
                _logger.Debug("Tentativo di connessione a etcd n." + iteration);
                etcdRootPdlEnabled = EtcdHelper.GetNodeFromKey(etcdKey);

                if (etcdRootPdlEnabled == null)
                {
                    /* In caso di nodo non recuperato incremento le iterazioni e passo alla prossima iterazione di loop dopo aver atteso l'intervallo */
                    _logger.Error("Errore durante la verifica dell'abilitazione.");
                    iteration++;
                    Thread.Sleep(retryIntervalSeconds * 1000);
                    continue;
                }

                /* Se ho il nodo comunico l'avvenuto recupero ed esco dal ciclo */
                _logger.Info("Comunicazione con ETCD per recuper abilitazione riuscita al tentativo n." + iteration);
                break;
            }

            if (etcdRootPdlEnabled != null && etcdRootPdlEnabled.action != "404" && Convert.ToBoolean(etcdRootPdlEnabled.node?.value))
            {
                _logger.Debug("Etcd key: " + etcdKey + " - Etcd value: " + etcdRootPdlEnabled.node.value.ToString());
            }

            return etcdRootPdlEnabled;
        }

        private bool ValidateConfig()
        {
            _logger.Info("Validazione delle configurazioni");
            Validator validator = ValidatorHandler.ValidateConfiguration();
            if (!validator.result)
            {
                foreach (string errorMessage in validator.errorMessages)
                {
                    _logger.Error("Validazione configurazione fallita: " + errorMessage);
                }
                return false;
            }
            return true;
        }

        public void OnTimer(object sender = null, ElapsedEventArgs args = null)
        {
            try
            {
                List<Document> availableDocuments = ConsumerHandler.GetAvailableDocuments(fraz);
                if (availableDocuments == null || availableDocuments.Count <= 0)
                {
                    return;
                }
                foreach (Document item in availableDocuments)
                {
                    WecomHelper.SendLinkToPrinter(printerName, item.url);
                    _logger.Info("Sending " + item.filename + " on printer " + printerName);
                }
            }
            catch (WecomException)
            {
                throw;
            }
            catch (Exception ex2)
            {
                _logger.Error(ex2.Message);
            }
        }

        protected override void OnContinue()
        {
            eventLog.WriteEntry("In OnContinue.");
        }

        protected override void OnStop()
        {
            ServiceStates.ServiceStatus serviceStatus = default(ServiceStates.ServiceStatus);
            serviceStatus.dwCurrentState = ServiceStates.ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(base.ServiceHandle, ref serviceStatus);
            serviceStatus.dwCurrentState = ServiceStates.ServiceState.SERVICE_STOPPED;
            SetServiceStatus(base.ServiceHandle, ref serviceStatus);
            _logger.Info("In OnStop.");
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStates.ServiceStatus serviceStatus);

        private void EnableTLS12()
        {
            try
            {
                using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework\\v4.0.30319"))
                {
                    if (registryKey2 == null)
                    {
                        return;
                    }
                    if ((int)registryKey2.GetValue("SchUseStrongCrypto", 0) != 1)
                    {
                        _logger.Debug("Imposto la chiave SchUseStrongCrypto a 1");
                        registryKey2.SetValue("SchUseStrongCrypto", 1, RegistryValueKind.DWord);
                    }
                    if ((int)registryKey2.GetValue("SystemDefaultTlsVersion", 0) != 1)
                    {
                        _logger.Debug("Imposto la chiave SystemDefaultTlsVersion a 1");
                        registryKey2.SetValue("SystemDefaultTlsVersion", 1, RegistryValueKind.DWord);
                    }
                }
                using RegistryKey registryKey3 = registryKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\SCHANNEL\\Protocols\\TLS 1.2\\Client");
                if (registryKey3 != null && (int)registryKey3.GetValue("Enabled", 0) != 1)
                {
                    _logger.Debug("Imposto la chiave Enabled a 1");
                    registryKey3.SetValue("Enabled", 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error Message: " + ex.Message);
                _logger.Error("StackTrace: " + ex.StackTrace);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.eventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).BeginInit();
            // 
            // RemotePrinterWorker
            // 
            this.ServiceName = "Remote Printer Worker";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).EndInit();

        }

        private Pdl GetPdlContext()
        {
            Pdl pdl = new Pdl();
            try
            {
                pdl = WecomHelper.GetPdlContext(byRegisterKey: true);
            }
            catch (Exception ex)
            {
                /* Stoppo il servizio se non recupero contesto PDL */
                _logger.Error(ex.Message);
                OnStop();
                return null;
            }

            return pdl;
        }
    }
}
