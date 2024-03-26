using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using RemotePrinterWorker.Models.Exceptions;
using RemotePrinterWorker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using RemotePrinterWorker.Models.Requests;
using RemotePrinterWorker.Models.Responses;

namespace RemotePrinterWorker.Helpers
{
    public static class WecomHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string token;

        public static Pdl GetPdlContext(bool byRegisterKey)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            Pdl result = null;
            try
            {
                if (byRegisterKey)
                {
                    _logger.Debug("ricerca con key W10:");
                    _logger.Debug("codUP: " + ConfigurationManager.AppSettings["w10RegisterKey_Fraz"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_codUP"]);
                    _logger.Debug("denUP: " + ConfigurationManager.AppSettings["w10RegisterKey_Fraz"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_denUP"]);
                    _logger.Debug("pdl: " + ConfigurationManager.AppSettings["w10RegisterKey_Pdl"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_PDL"]);
                    empty = (string)Registry.GetValue(ConfigurationManager.AppSettings["w10RegisterKey_Fraz"], ConfigurationManager.AppSettings["RegisterKey_codUP"], string.Empty);
                    empty2 = (string)Registry.GetValue(ConfigurationManager.AppSettings["w10RegisterKey_Fraz"], ConfigurationManager.AppSettings["RegisterKey_denUP"], string.Empty);
                    empty3 = (string)Registry.GetValue(ConfigurationManager.AppSettings["w10RegisterKey_Pdl"], ConfigurationManager.AppSettings["RegisterKey_PDL"], string.Empty);
                    _logger.Debug("Valore chiave di registro codUP W10: " + empty);
                    _logger.Debug("Valore chiave di registro denUP W10: " + empty2);
                    _logger.Debug("Valore chiave di registro pdl W10: " + empty3);
                    if (string.IsNullOrEmpty(empty) || string.IsNullOrEmpty(empty2) || string.IsNullOrEmpty(empty3))
                    {
                        _logger.Debug("ricerca con key W7:");
                        _logger.Debug("codUP: " + ConfigurationManager.AppSettings["w7RegisterKey_Fraz"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_codUP"]);
                        _logger.Debug("denUP: " + ConfigurationManager.AppSettings["w7RegisterKey_Fraz"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_denUP"]);
                        _logger.Debug("pdl: " + ConfigurationManager.AppSettings["w7RegisterKey_Pdl"] + "; Value: " + ConfigurationManager.AppSettings["RegisterKey_PDL"]);
                        empty = (string)Registry.GetValue(ConfigurationManager.AppSettings["w7RegisterKey_Fraz"], ConfigurationManager.AppSettings["RegisterKey_codUP"], string.Empty);
                        empty2 = (string)Registry.GetValue(ConfigurationManager.AppSettings["w7RegisterKey_Fraz"], ConfigurationManager.AppSettings["RegisterKey_denUP"], string.Empty);
                        empty3 = (string)Registry.GetValue(ConfigurationManager.AppSettings["w7RegisterKey_Pdl"], ConfigurationManager.AppSettings["RegisterKey_PDL"], string.Empty);
                        _logger.Debug("Valore chiave di registro codUP: " + empty);
                        _logger.Debug("Valore chiave di registro denUP: " + empty2);
                        _logger.Debug("Valore chiave di registro pdl: " + empty3);
                    }
                    if (string.IsNullOrEmpty(empty) || string.IsNullOrEmpty(empty2) || string.IsNullOrEmpty(empty3))
                    {
                        string text = "Una o più chiavi di registro mancanti: ";
                        if (string.IsNullOrEmpty(empty))
                        {
                            text += "chiave codUP assente; ";
                        }
                        if (string.IsNullOrEmpty(empty2))
                        {
                            text += "chiave denUP assente; ";
                        }
                        if (string.IsNullOrEmpty(empty3))
                        {
                            text += "chiave pdl assente; ";
                        }
                        throw new Exception(text);
                    }
                    result = new Pdl
                    {
                        fraz = empty + empty2,
                        pdl = empty3.PadLeft(2, '0')
                    };
                }
            }
            catch (Exception message)
            {
                _logger.Error(message);
                throw;
            }
            return result;
        }

        public static Pdl GetPdlContext()
        {
            Pdl pdl = null;
            try
            {
                string requestUriString = ConfigurationManager.AppSettings["getPdlContextEndpoint"];
                string text = string.Empty;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
                if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
                {
                    httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
                }
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                    }
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    text = streamReader.ReadToEnd();
                }
                _logger.Info(text);
                return JsonConvert.DeserializeObject<PdlContext>(text).ctx;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        public static List<RemotePrinter> GetRemotePrinters()
        {
            string requestUriString = ConfigurationManager.AppSettings["getRemotePrinters"];
            RemotePrinters remotePrinters = null;
            try
            {
                string text = string.Empty;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
                if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
                {
                    httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
                }
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Debug(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                        throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                    }
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    if (bool.TryParse(ConfigurationManager.AppSettings["FakePrinter"], out var result) && result)
                    {
                        remotePrinters = new RemotePrinters
                        {
                            remotePRinters = new List<RemotePrinter>
                        {
                            new RemotePrinter
                            {
                                printerName = "FakePrinter_1"
                            },
                            new RemotePrinter
                            {
                                printerName = "FakePrinter_2"
                            },
                            new RemotePrinter
                            {
                                printerName = "FakePrinter_3"
                            }
                        }
                        };
                        return remotePrinters?.remotePRinters;
                    }
                    text = streamReader.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(text))
                {
                    _logger.Debug(text);
                    remotePrinters = JsonConvert.DeserializeObject<RemotePrinters>(text);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new WecomException(ex.Message);
            }
            return remotePrinters?.remotePRinters;
        }

        public static void SendLinkToPrinter(string printerName, string link)
        {
            //IL_00c6: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d0: Expected O, but got Unknown
            int num = int.Parse(ConfigurationManager.AppSettings["WebSocketWaitTime"]);
            try
            {
                string uri = ConfigurationManager.AppSettings["wecomAddres"];
                if (!link.Contains(ConfigurationManager.AppSettings["domain"]))
                {
                    _logger.Error("ERRORE! Dominio non riconosciuto: " + link);
                    return;
                }
                string file = downloadFile(link);
                if (file == null || file.Length == 0)
                {
                    _logger.Error("Download " + link + " non riuscito");
                    return;
                }
                WebSocket ws = new WebSocket(uri, Array.Empty<string>());
                try
                {
                    ws.WaitTime = TimeSpan.FromSeconds(num);
                    ws.OnOpen += delegate
                    {
                        _logger.Info("Connessione alla webSocket " + uri + " riuscita");
                        PrintDownloadReq value = new PrintDownloadReq
                        {
                            cmd = "printer.printPdf",
                            payload = new Models.Requests.Payload
                            {
                                printerName = printerName,
                                file = file
                            }
                        };
                        ws.Send(JsonConvert.SerializeObject(value));
                    };
                    ws.OnMessage += delegate (object sender, MessageEventArgs e)
                    {
                        _logger.Info("Received from Server: " + e.Data);
                        PrintDownloadResp printDownloadResp = JsonConvert.DeserializeObject<PrintDownloadResp>(e.Data);
                        if (printDownloadResp != null)
                        {
                            if (printDownloadResp == null)
                            {
                                _logger.Error("No response available");
                                throw new Exception("No response available");
                            }
                            if (printDownloadResp.status == "200" && string.Equals(printDownloadResp.payload.result, "Ok", StringComparison.InvariantCultureIgnoreCase))
                            {
                                _logger.Info("File ricevuto correttamente dalla webSocket");
                            }
                            else
                            {
                                if (!(printDownloadResp.status == "202"))
                                {
                                    _logger.Error("Problema tecnico del servizio");
                                    throw new Exception("Problema tecnico del servizio");
                                }
                                _logger.Info("No document available");
                            }
                        }
                        else
                        {
                            _logger.Error("No response available");
                        }
                        ws.Close();
                    };
                    ws.OnClose += delegate
                    {
                        _logger.Info("Chiusa connessione a " + uri);
                    };
                    ws.Connect();
                }
                finally
                {
                    if (ws != null)
                    {
                        ((IDisposable)ws).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new WecomException(ex.Message);
            }
        }

        public static string getToken()
        {
            try
            {
                if (token != null && token.Length > 0)
                {
                    return token;
                }
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:9181/tokeniam/get");
                if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
                {
                    httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
                }
                using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                }
                using Stream stream = httpWebResponse.GetResponseStream();
                using StreamReader streamReader = new StreamReader(stream);
                token = streamReader?.ReadToEnd()?.Replace("{", "").Replace("}", "").Replace("\"tokeniam\":\"", "")
                    .Replace("\"", "");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new WecomException(ex.Message);
            }
            _logger.Debug(token);
            return token;
        }

        public static void resetToken()
        {
            token = string.Empty;
        }

        public static string downloadFile(string url)
        {
            string text = string.Empty;
            _logger.Info("doc url: " + url);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.CookieContainer.Add(new Cookie("SMSESSION", getToken(), "/", ConfigurationManager.AppSettings["domain"]));
            if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
            {
                httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
            }
            byte[] array;
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                }
                using Stream input = httpWebResponse.GetResponseStream();
                using BinaryReader binaryReader = new BinaryReader(input);
                int count = (int)httpWebResponse.ContentLength;
                array = binaryReader.ReadBytes(count);
                binaryReader.Close();
            }
            if (array != null && array.Length != 0)
            {
                _logger.Debug("FILE Downloaded bytes: " + array.Length);
                text = Convert.ToBase64String(array, 0, array.Length);
            }
            _logger.Debug("FILE Downloaded Base64 Length: " + text.Length);
            return text;
        }
    }
}
