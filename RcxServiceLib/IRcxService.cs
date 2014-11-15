using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace Rcx
{
    [ServiceContract]
    public interface IRcxService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Command", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string InvokeCommand(string path, string[] args = null);

        [OperationContract]
        [WebGet(UriTemplate = "/Command/{guid}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Command GetCommand(string guid);

        [OperationContract]
        [WebGet(UriTemplate = "/File/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetFile(string path);

        [OperationContract]
        [WebInvoke(UriTemplate = "/File/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SendFile(string path, Stream data);

        [OperationContract]
        [WebInvoke(UriTemplate = "/File/{*path}", Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void DeleteFile(string path);

        [OperationContract]
        [WebGet(UriTemplate = "/FileSystemItem/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        FileSystemItem GetFileSystemItem(string path);
    }
}
