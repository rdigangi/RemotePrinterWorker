using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace RemotePrinterWorker
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private IContainer components;

        private ServiceProcessInstaller serviceProcessInstaller1;

        private ServiceInstaller serviceInstaller1;

        public ProjectInstaller()
        {
            InitializeComponent();
            serviceInstaller1.AfterInstall += Autorun_AfterServiceInstall;
        }

        private void Autorun_AfterServiceInstall(object sender, InstallEventArgs e)
        {
            using ServiceController serviceController = new ServiceController(((ServiceInstaller)sender).ServiceName);
            serviceController.Start();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
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
            serviceProcessInstaller1 = new ServiceProcessInstaller();
            serviceInstaller1 = new ServiceInstaller();
            serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller1.Password = null;
            serviceProcessInstaller1.Username = null;
            serviceProcessInstaller1.AfterInstall += serviceProcessInstaller1_AfterInstall;
            serviceInstaller1.Description = "A service to manage the remote printers by ODA";
            serviceInstaller1.DisplayName = "Remote Printer Worker";
            serviceInstaller1.ServiceName = "Remote Printer Worker";
            serviceInstaller1.StartType = ServiceStartMode.Automatic;
            serviceInstaller1.AfterInstall += serviceInstaller1_AfterInstall;
            base.Installers.AddRange(new Installer[2] { serviceProcessInstaller1, serviceInstaller1 });
        }
    }
}
