using System;
using System.IO;

namespace Rcx
{
    // NOT USED CURRENTLY
    public class FileManager
    {
        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void SendFile(string filename, Stream stream)
        {
            using (FileStream targetStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(targetStream);
            }
        }

        public FileSystemItem GetFileSystemItem(string path)
        {
            return new FileSystemItem(path);
        }
    }
}
