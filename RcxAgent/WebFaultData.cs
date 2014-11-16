using System;
using System.Runtime.Serialization;

namespace Rcx
{
    [DataContract]
    public class WebFaultData
    {
        public WebFaultData(string message, string details)
        {
            Message = message;
            Details = details;
        }

        public WebFaultData(Exception e)
        {
            Message = "Error";
            Details = e.ToString();
        }

        [DataMember]
        public string Message { get; private set; }

        [DataMember]
        public string Details { get; private set; }
    }
}
