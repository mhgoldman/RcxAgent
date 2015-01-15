using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Rcx
{
    public class Callbacker
    {
        private readonly int MAX_CALLBACK_FREQ_SECS = 5;
        private object _lock = new object();

        private DateTime LastCallbackTime
        {
            get;
            set;
        }

        private int LastCallbackMessageLength
        {
            get;
            set;
        }

        private string CallbackUrl
        {
            get;
            set;
        }

        private string LastCallbackMessage
        {
            get;
            set;
        }

        private Command Command
        {
            get;
            set;
        }

        public Callbacker(Command command, string callbackUrl)
        {
            Command = command;
            CallbackUrl = callbackUrl;
            LastCallbackTime = DateTime.MinValue;
        }

        public void RunPeriodic()
        {
            if (DateTime.Now > LastCallbackTime.AddSeconds(MAX_CALLBACK_FREQ_SECS))
            {
                Run();
            }
        }

        public void Run()
        {
            lock(_lock)
            {
                try
                {
                    Log.Verbose("Beginning callback for command {command}", Command.Guid);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string json = serializer.Serialize(Command);

                    if (json == LastCallbackMessage)
                    {
                        Log.Verbose("Duplicate callback message will not be sent.");
                        return;
                    }

                    LastCallbackTime = DateTime.Now;

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpStatusCode result = client.PutAsync(CallbackUrl, content).Result.StatusCode;

                    Log.Information("Callback completed for command {command} with result {result}", Command.Guid, result);

                    LastCallbackMessage = json;
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Exception in Callback");
                }
            }
        }
    }
}
