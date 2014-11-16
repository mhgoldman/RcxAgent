using System;
using System.IO;

namespace Rcx
{
    public static class FileManager
    {
        public static Stream GetFile(string path, out string filename)
        {
            filename = Path.GetFileName(path);

            return new FileStream(path, FileMode.Open);
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public static void SendFile(string filename, Stream stream)
        {
            using (FileStream targetStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(targetStream);
            }
        }

        public static FileSystemItem GetFileSystemItem(string path)
        {
            return new FileSystemItem(path);
        }
    }
}
