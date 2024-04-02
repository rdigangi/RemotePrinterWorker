using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace RemotePrinterWorker
{
    internal static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            try
            {
                _logger.Info("Application started as service App.");
                ServiceBase.Run(new ServiceBase[1]
                {
            new RemotePrinterWorker()
                });
            }
            catch(Exception ex)
            {
                _logger.Error("Errore in void Main: " + ex.Message);
                _logger.Error("StackTrace: " + ex.StackTrace);
            }
            
        }
    }
}
