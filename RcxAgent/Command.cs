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
        private object callbackLock;

        [DataMember]
        public string Guid
        {
            get;
            private set;
        }

        [DataMember]
        public string Path
        {
            get;
            private set;
        }

        [DataMember]
        public string[] Args
        {
            get;
            private set;
        }

        private int _pid;
        [DataMember]
        public int Pid
        {
            get
            {
                if (_pid == 0)
                {
                    _pid = process.Id;
                }

                return _pid;
            }

            private set { } //required by serialization
        }

        private bool _hasExited;
        [DataMember]
        public bool HasExited
        {
            get
            {
                if (!_hasExited)
                {
                    _hasExited = process.HasExited;
                }

                return _hasExited;
            }

            private set { } //required by serialization
        }

        private string _standardOutput;
        [DataMember]
        public string StandardOutput
        {
            get
            {
                _standardOutput = stdOutSb.ToString();

                return _standardOutput;
            }

            private set { } //required by serialization
        }

        private string _standardError;
        [DataMember]
        public string StandardError
        {
            get
            {
                _standardError = stdErrSb.ToString();

                return _standardError;
            }

            private set { } //required by serialization
        }

        int _exitCode;
        [DataMember]
        public int ExitCode
        {
            get
            {
                if (_exitCode == 0 && HasExited)
                {
                    _exitCode = process.ExitCode;
                }

                return _exitCode;
            }
            private set { } //required by serialization
        }

        private Callbacker Callbacker
        {
            get;
            set;
        }

        private bool FinalCallbackRan
        {
            get;
            set;
        }

        public Command(string guid, string path, string[] args, string callbackUrl = null)
        {
            Guid = guid;
            Path = path;
            Args = args;

            stdOutSb = new StringBuilder();
            stdErrSb = new StringBuilder();

            Callbacker = new Callbacker(this, callbackUrl);
            callbackLock = new object();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            if (args != null && args.Length > 0)
            {
                startInfo.Arguments = String.Join(" ", args);
            }
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

        public void Kill()
        {
            process.Kill();
        }

        private void RunCallback()
        {
            //While the process is running, we want the callback to run only periodically to provide the latest output.
            //Once the process exits, the output/error data handlers may keep firing as output/errors come back. There's no way to know when that's finished..
            //To ensure that we capture the Command in its final state, we have to transmit the whole thing on every change after the process has exited.
            //The Callbacker helps with this somewhat by ensuring that the callback message being sent isn't identical to the previous one sent.
            lock (callbackLock)
            {
                Log.Verbose("Got callback lock");
                if (HasExited) 
                {
                    Callbacker.Run();
                }
                else
                {
                    Callbacker.RunPeriodic();
                }
                Log.Verbose("Surrendering callback lock");
            }
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
                    Log.Verbose("Command {Command} wrote to StdOut: {Data}", Guid, outLine.Data);
                }

                RunCallback();
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
                    Log.Verbose("Command {Command} wrote to StdErr: {Data}", Guid, outLine.Data);
                }

                RunCallback();
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
                Log.Information("Command {Command} completed with exit code {ExitCode}", Guid, ExitCode);
                Log.Verbose("Command properties: {@Command}", this);

                RunCallback();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Exception in ExitedEventHandler");
            }
        }
        #endregion
    }
}
