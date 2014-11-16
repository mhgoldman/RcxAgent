﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Web;
using System.Collections.Concurrent;

namespace Rcx
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode=InstanceContextMode.Single)]
    public class RcxService : IRcxService
    {
        #region command management
        public string InvokeCommand(string path, string[] args = null)
        {
            string guid = Guid.NewGuid().ToString();

            try
            {
                CommandManager.Default.AddCommand(guid, path, args);
            }
            catch (ArgumentException e)
            {
                ThrowWebFault(e, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }
            return guid;
        }

        public Command GetCommand(string guid)
        {
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
            return CommandManager.Default.GetCommands();
        }
        #endregion

        #region file management
        public Stream GetFile(string path)
        {
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

        #region helpers
        private void ThrowWebFault(string message, string details, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            throw new WebFaultException<WebFaultData>(new WebFaultData(message, details), statusCode);
        }

        private void ThrowWebFault(Exception e, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            throw new WebFaultException<WebFaultData>(new WebFaultData(e), statusCode);
        }
        #endregion
    }
}