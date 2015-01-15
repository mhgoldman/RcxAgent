using System;
using System.ServiceModel;
using System.ServiceProcess;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Rcx
{
    public partial class RcxAgentWinService : ServiceBase
    {
        public ServiceHost serviceHost = null;

        private static DirectoryInfo _logDir;
        private static DirectoryInfo LogDir
        {
            get
            {   if (_logDir == null)
                {
                    _logDir = new DirectoryInfo(Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"), "RcxAgent", "Logs"));
                    if (!_logDir.Exists)
                    {
                        _logDir.Create();
                    }
                }

                return _logDir;
            }
        }

        public RcxAgentWinService()
        {
            ServiceName = "RcxAgentSvc";
            CanShutdown = true;
        }

        public static void Main()
        {
            LogEventLevel logLevel = Serilog.Events.LogEventLevel.Information;
            
            #if DEBUG
            logLevel = Serilog.Events.LogEventLevel.Verbose;
            #endif

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(Path.Combine(LogDir.ToString(), "log.txt"), restrictedToMinimumLevel: logLevel, fileSizeLimitBytes: 5242880, retainedFileCountLimit: 10 )
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServiceBase.Run(new RcxAgentWinService());
        }

        protected override void OnStart(string[] args)
        {
            Log.Information("Starting Windows service {ServiceName}", ServiceName);

            try
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }

                serviceHost = new ServiceHost(typeof(RcxService));
                serviceHost.Open();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in OnStart");
            }
        }

        protected override void OnStop()
        {
            Log.Information("Stopping Windows service {ServiceName}", ServiceName);

            try
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                    serviceHost = null;
                }

                CommandManager.Default.OnMayday();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in OnStop");
            }

        }

        protected override void OnShutdown()
        {
            OnStop();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            Log.Error(exception, "Unhandled exception");
        }

    }
}
