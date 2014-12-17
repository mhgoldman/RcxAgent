using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.Collections.Concurrent;

namespace Rcx
{
    [ServiceContract]
    public interface IRcxService
    {
        #region command management
        [OperationContract]
        [WebInvoke(UriTemplate = "/Commands", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Command InvokeCommand(string path, string[] args = null, string callbackUrl = null);

        [OperationContract]
        [WebGet(UriTemplate = "/Commands", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ConcurrentDictionary<string, Command> GetCommands();

        [OperationContract]
        [WebGet(UriTemplate = "/Commands/{guid}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Command GetCommand(string guid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Commands/{guid}", Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void KillCommand(string guid);
        #endregion

        #region file management
        [OperationContract]
        [WebGet(UriTemplate = "/Files/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetFile(string path);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Files/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SendFile(string path, Stream data);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Files/{*path}", Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void DeleteFile(string path);

        [OperationContract]
        [WebGet(UriTemplate = "/FileSystemItems/{*path}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        FileSystemItem GetFileSystemItem(string path);
        #endregion
    }
}
