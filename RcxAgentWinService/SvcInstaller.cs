using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Rcx
{
    [RunInstaller(true)]
    class SvcInstaller : Installer
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
