using System;
using System.Runtime.Serialization;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using Serilog;

namespace Rcx
{
    [DataContract]
    public class Command
    {
        private Process process;
        private StringBuilder stdOutSb, stdErrSb;

        [DataMember]
        public string Guid
        {
            get;
            private set;
        }

        [DataMember]
        public int Pid
        {
            get;
            set;
        }

        [DataMember]
        public bool HasExited
        {
            get;
            set;
        }

        [DataMember]
        public string StandardOutput
        {
            get;
            set;
        }

        [DataMember]
        public string StandardError
        {
            get;
            set;
        }

        [DataMember]
        public int ExitCode
        {
            get;
            set;
        }

        private bool SentFinalCallback
        {
            get;
            set;
        }

        private Callbacker Callbacker
        {
            get;
            set;
        }

        public Command(string guid, string path, string[] args, string callbackUrl = null)
        {
            Guid = guid;

            stdOutSb = new StringBuilder();
            stdErrSb = new StringBuilder();

            Callbacker = new Callbacker(this, callbackUrl);
            SentFinalCallback = false;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.Arguments = String.Join(" ", args);
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
            process.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
            process.Exited += new EventHandler(ExitedEventHandler);

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            Log.Information("Created new command {Command}", Guid);
            Log.Verbose("New command properties: {@Command}", this);
        }

        public Command Update()
        {
            HasExited = process.HasExited;
            Pid = process.Id;
            if (HasExited)
            {
                ExitCode = process.ExitCode;
            }
            StandardError = stdErrSb.ToString();
            StandardOutput = stdOutSb.ToString();

            if (HasExited && !SentFinalCallback)
            {
                SentFinalCallback = true;
                Callbacker.Run();
            }
            else
            {
                Callbacker.RunPeriodic();
            }

            return this;
        }

        public void Kill()
        {
            process.Kill();
        }

        #region event handlers
        //These run outside the WCF request/response cycle. If they throw exceptions, the service will crash rather than rendering nice little WebFaults.
        //So we handle exceptions explicitly here.
        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    stdOutSb.Append(Environment.NewLine + outLine.Data);
                    Log.Verbose("Command {Command} wrote to StdOut: ", Guid, outLine.Data);
                }

                Update();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in OutputDataHandler");
            }
        }

        private void ErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    stdErrSb.Append(Environment.NewLine + outLine.Data);
                    Log.Verbose("Command {Command} wrote to StdErr: ", Guid, outLine.Data);
                }

                Update();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in ErrorDataHandler");
            }
        }

        private void ExitedEventHandler(object sender, EventArgs e)
        {
            try
            {
                Update();
                Log.Information("Command {Command} completed with exit code {ExitCode}", Guid, ExitCode);
                Log.Verbose("Command properties: {@Command}", this);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in ExitedEventHandler");
            }
        }
        #endregion
    }
}
