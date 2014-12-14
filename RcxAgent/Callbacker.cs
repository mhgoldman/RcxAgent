using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Rcx
{
    public class Callbacker
    {
        private DateTime LastCallbackTime
        {
            get;
            set;
        }

        private string CallbackUrl
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
            if (DateTime.Now > LastCallbackTime.AddSeconds(5))
            {
                Run();
            }
        }

        public void Run()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(Command);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpStatusCode result = client.PutAsync(CallbackUrl, content).Result.StatusCode;
            LastCallbackTime = DateTime.Now;

            Log.Information("Callback completed for command {command} with result {result}", Command.Guid, result);
        }
    }
}
