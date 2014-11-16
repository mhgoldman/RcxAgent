using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Rcx
{
    [RunInstaller(true)]
    public class SvcInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public SvcInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "RcxAgentSvc";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
