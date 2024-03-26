using RemotePrinterWorker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Handlers
{
    public static class ValidatorHandler
    {
        public static Validator ValidateConfiguration()
        {
            Validator validator = new Validator
            {
                result = true,
                errorMessages = new List<string>()
            };

            int result = 0;
            
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxRetry"], out result))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'MaxRetry'. ");
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["RetryIntervalSeconds"], out result) && result > 0)
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'RetryIntervalSeconds'. ");
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["webSocketTimeOutSeconds"], out result))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'webSocketTimeOutSeconds'. ");
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["WebSocketWaitTime"], out result))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'WebSocketWaitTime'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["getPdlContextEndpoint"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'getPdlContextEndpoint'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["etcdRemotePrintEnabled"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'etcdRemotePrintEnabled'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["domain"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'domain'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["etcdEndpoint"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'etcdEndpoint'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["getRemotePrinters"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'getRemotePrinters'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["etcdRemotePrintPollingInterval"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'etcdRemotePrintPollingInterval'. ");
            }
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["wecomAddres"]))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'wecomAddres'. ");
            }
            if (!bool.TryParse(ConfigurationManager.AppSettings["DisableSSL"], out var _))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'DisableSSL'. ");
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["TimerSDP"], out result) && result > 0)
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'TimerSDP'. ");
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxRetrySDP"], out result))
            {
                validator.result = false;
                validator.errorMessages.Add("Invalid value for config key 'MaxRetrySDP'. ");
            }
            return validator;
        }
    }
}
