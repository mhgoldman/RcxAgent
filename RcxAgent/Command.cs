using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Rcx
{
    [DataContract]
    public class Command
    {
        private Process process;
        private StringBuilder stdOutSb, stdErrSb;

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

        public Command(string path, string[] args)
        {
            stdOutSb = new StringBuilder();
            stdErrSb = new StringBuilder();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.Arguments = String.Join(" ", args);
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            process = new Process();
            process.StartInfo = startInfo;
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
            process.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            Update();
        }

        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                stdOutSb.Append(Environment.NewLine + outLine.Data);
            }
        }

        private void ErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                stdErrSb.Append(Environment.NewLine + outLine.Data);
            }
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

            return this;
        }

        public void Kill()
        {
            process.Kill();
        }
    }
}
