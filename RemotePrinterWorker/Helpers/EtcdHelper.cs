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

namespace RemotePrinterWorker.Helpers
{
    public static class EtcdHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static EtcdRoot GetNodeFromKey(string key, bool recursive = false)
        {
            EtcdRoot result = null;
            string text = ConfigurationManager.AppSettings["etcdEndpoint"];
            try
            {
                string value = string.Empty;
                HttpWebRequest httpWebRequest = (recursive ? ((HttpWebRequest)WebRequest.Create(text + key + "?recursive=true")) : ((HttpWebRequest)WebRequest.Create(text + key)));
                httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
                if (bool.Parse(ConfigurationManager.AppSettings["DisableSSL"]))
                {
                    httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
                }
                using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return new EtcdRoot
                    {
                        action = "404"
                    };
                }
                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Error(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                }
                else
                {
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        using StreamReader streamReader = new StreamReader(stream);
                        if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(httpWebResponse.StatusCode.ToString() + " " + httpWebResponse.StatusDescription);
                        }
                        value = streamReader.ReadToEnd();
                    }
                    result = JsonConvert.DeserializeObject<EtcdRoot>(value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
            return result;
        }
    }
}
