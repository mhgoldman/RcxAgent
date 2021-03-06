﻿using System;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Web;
using System.Collections.Concurrent;
using Serilog;
using System.ServiceModel.Channels;

namespace Rcx
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode=InstanceContextMode.Single)]
    public class RcxService : IRcxService
    {
        #region command management
        public Command InvokeCommand(string path, string[] args = null, string callbackUrl = null, string callbackToken = null)
        {
            Log.Information("InvokeCommand call from {Ip}", GetClientIp());

            Command c = null;
            string guid = Guid.NewGuid().ToString();

            try
            {
                c = CommandManager.Default.AddCommand(guid, path, args, callbackUrl, callbackToken);
            }
            catch (ArgumentException e)
            {
                ThrowWebFault(e, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }

            return c;
        }

        public Command GetCommand(string guid)
        {
            Log.Information("GetCommand call for {Guid} from {Ip}", guid, GetClientIp());

            Command c = null;

            try
            {
                c = CommandManager.Default.GetCommand(guid);
            }
            catch (ArgumentException e)
            {
                ThrowWebFault(e, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }

            return c;
        }

        public void KillCommand(string guid)
        {
            Log.Information("KillCommand call for {Guid} from {Ip}", guid, GetClientIp());

            Command c = null;

            try
            {
                c = CommandManager.Default.GetCommand(guid);
                c.Kill();
            }
            catch (ArgumentException e)
            {
                ThrowWebFault(e, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            } 
        }

        public ConcurrentDictionary<string, Command> GetCommands()
        {
            Log.Information("GetCommands call from {Ip}", GetClientIp());

            return CommandManager.Default.GetCommands();
        }
        #endregion

        #region file management
        public Stream GetFile(string path)
        {
            Log.Information("GetFile call for {Path} from {Ip}", path, GetClientIp());

            Stream stream = null;
            string filename;

            try
            {
                stream = FileManager.GetFile(path, out filename);

                OutgoingWebResponseContext responseContext = WebOperationContext.Current.OutgoingResponse;
                responseContext.Headers.Add("Content-Disposition", String.Format("attachment; filename={0}", filename));
                responseContext.Headers.Add("Content-Type", MimeMapping.GetMimeMapping(filename));
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }

            return stream; 
        }

        public void DeleteFile(string path)
        {
            Log.Information("DeleteFile call for #{Path} from {Ip}", path, GetClientIp());

            try
            {
                FileManager.DeleteFile(path);
            }
            catch (UnauthorizedAccessException)
            {
                ThrowWebFault("Access Denied", String.Format("Access to delete file {0} was denied.", path), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }
        }

        public void SendFile(string filename, Stream stream)
        {
            Log.Information("SendFile call for {Filename} from {Ip}", filename, GetClientIp());

            try
            {
                FileManager.SendFile(filename, stream);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }
        }

        public FileSystemItem GetFileSystemItem(string path)
        {
            Log.Information("GetFileSystemItem call for {Path} from {Ip}", path, GetClientIp());

            FileSystemItem item = null;

            try
            {
                item = FileManager.GetFileSystemItem(path);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }

            return item;
        }
        #endregion

        #region administrative
        public string Ping()
        {
            return "Pong";
        }
        #endregion

        #region helpers
        private void ThrowWebFault(string message, string details, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            WebFaultData webFaultData = new WebFaultData(message, details);
            Exception exception = new WebFaultException<WebFaultData>(webFaultData, statusCode);
            Log.Error(exception, "Throwing WebFaultException: {@WebFaultData}", webFaultData);
            throw exception;

        }

        private void ThrowWebFault(Exception e, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            WebFaultData webFaultData = new WebFaultData(e);
            Exception exception = new WebFaultException<WebFaultData>(webFaultData, statusCode);
            Log.Error(exception, "Throwing WebFaultException: {@WebFaultData}", webFaultData);
            throw exception;
        }

        private string GetClientIp()
        {
            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            return endpoint.Address;
        }
        #endregion
    }
}
