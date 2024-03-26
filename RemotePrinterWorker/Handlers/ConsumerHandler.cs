using log4net;
using Newtonsoft.Json;
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
using RemotePrinterWorker.Helpers;

namespace RemotePrinterWorker.Handlers
{
    public static class ConsumerHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string consumerUrl = ConfigurationManager.AppSettings["consumerService"];

        public static List<Document> GetAvailableDocuments(string fraz)
        {
            List<Document> result = null;
            _logger.Info("ConsumerHandler.GetAvailableDocuments");
            try
            {
                string text = consumerUrl.Replace("#fraz", fraz);
                _logger.Info("Url: " + text);
                string text2 = string.Empty;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(new Cookie("SMSESSION", WecomHelper.getToken(), "/", ConfigurationManager.AppSettings["domain"]));
                if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
                {
                    httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
                }
                _logger.Debug("Address: " + httpWebRequest.Address.ToString());
                foreach (X509Certificate clientCertificate in httpWebRequest.ClientCertificates)
                {
                    _logger.Debug("Cert Subject: " + clientCertificate.Subject.ToString());
                    _logger.Debug("Cert Issuer: " + clientCertificate.Issuer.ToString());
                }
                foreach (object header in httpWebRequest.Headers)
                {
                    _logger.Debug("Header value: " + header.ToString());
                }
                foreach (object cookie in httpWebRequest.CookieContainer.GetCookies(httpWebRequest.RequestUri))
                {
                    _logger.Debug("cookie value: " + cookie.ToString());
                }
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    _logger.Debug("StatusCode: " + httpWebResponse.StatusCode);
                    if (httpWebResponse.StatusCode == HttpStatusCode.Accepted)
                    {
                        _logger.Info("Nessun Messaggio in Coda");
                        return result;
                    }
                    if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                    }
                    using Stream stream = httpWebResponse.GetResponseStream();
                    using StreamReader streamReader = new StreamReader(stream);
                    text2 = streamReader.ReadToEnd();
                }
                _logger.Debug(text2);
                GetAvailableDocumentsRepsonse getAvailableDocumentsRepsonse = JsonConvert.DeserializeObject<GetAvailableDocumentsRepsonse>(text2);
                if (getAvailableDocumentsRepsonse.errors != null && getAvailableDocumentsRepsonse.errors.Count > 0)
                {
                    _logger.Error("Errore nel recupero del link: " + getAvailableDocumentsRepsonse.errors.First().detail);
                    throw new Exception("Errore nel recupero del link: " + getAvailableDocumentsRepsonse.errors.First().detail);
                }
                return getAvailableDocumentsRepsonse.data.documents;
            }
            catch (Exception ex)
            {
                _logger.Error("ERRORE: " + ex.Message + " Stacktrace: " + ex.StackTrace);
                WecomHelper.resetToken();
                throw ex;
            }
        }
    }
}
