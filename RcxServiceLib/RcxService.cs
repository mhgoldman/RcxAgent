using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Web;

namespace Rcx
{
    public class RcxService : IRcxService
    {
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
                CommandManager.Default.GetCommand(guid);
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

        public Stream GetFile(string path)
        {
            if (!File.Exists(path))
            {
                ThrowWebFault("File Not Found", String.Format("The file {0} was not found.", path), HttpStatusCode.NotFound);
            }

            FileStream stream = null;

            try
            {
                FileInfo file = new FileInfo(path);

                OutgoingWebResponseContext responseContext = WebOperationContext.Current.OutgoingResponse;
                responseContext.Headers.Add("Content-Disposition", String.Format("attachment; filename={0}", file.Name));
                responseContext.Headers.Add("Content-Type", MimeMapping.GetMimeMapping(file.Name));

                stream = new FileStream(file.FullName, FileMode.Open);
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }

            return stream; 
        }

        public void DeleteFile(string path)
        {
            if (!File.Exists(path))
            {
                ThrowWebFault("File Not Found", String.Format("The file {0} was not found.", path), HttpStatusCode.NotFound);
            }

            try
            {
                File.Delete(path);
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
                using (FileStream targetStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(targetStream);
                }
            }
            catch (Exception e)
            {
                ThrowWebFault(e);
            }
        }

        public FileSystemItem GetFileSystemItem(string path)
        {
            return new FileSystemItem(path);
        }

        #region exception helpers
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
