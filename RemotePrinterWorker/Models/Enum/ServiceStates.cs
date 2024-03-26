using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePrinterWorker.Models.Enum
{
    public class ServiceStates
    {
        public enum ServiceState
        {
            SERVICE_STOPPED = 1,
            SERVICE_START_PENDING,
            SERVICE_STOP_PENDING,
            SERVICE_RUNNING,
            SERVICE_CONTINUE_PENDING,
            SERVICE_PAUSE_PENDING,
            SERVICE_PAUSED
        }

        public struct ServiceStatus
        {
            public int dwServiceType;

            public ServiceState dwCurrentState;

            public int dwControlsAccepted;

            public int dwWin32ExitCode;

            public int dwServiceSpecificExitCode;

            public int dwCheckPoint;

            public int dwWaitHint;
        }
    }
}
