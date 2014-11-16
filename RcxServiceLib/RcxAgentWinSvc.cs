using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Rcx
{
    public partial class RcxAgentWinService : ServiceBase
    {
        public ServiceHost serviceHost = null;

        public RcxAgentWinService()
        {
            ServiceName = "RcxAgentSvc";
        }

        public static void Main()
        {
            ServiceBase.Run(new RcxAgentWinService());
        }

        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            serviceHost = new ServiceHost(typeof(RcxService));
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
