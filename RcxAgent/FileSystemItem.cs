using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;

namespace Rcx
{
    public enum FileSystemItemType { File, Directory };

    [DataContract]
    public class FileSystemItem
    {
        [DataMember]
        public string Path
        {
            get;
            private set;
        }

        FileSystemItemType _typeEnum;

        [DataMember]
        public string Type
        {
            get
            {
                if (_typeEnum == FileSystemItemType.File)
                {
                    return "File";
                }
                else
                {
                    return "Directory";
                }
            }
            private set
            {
                if (value == "File")
                {
                    _typeEnum = FileSystemItemType.File;
                }
                else
                {
                    _typeEnum = FileSystemItemType.Directory;
                }
            }
        }

        [DataMember]
        public List<FileSystemItem> Children
        {
            get;
            private set;
        }

        [DataMember]
        public DateTime LastModifiedTime
        {
            get;
            private set;
        }

        [DataMember]
        public long Size
        {
            get;
            private set;
        }

        public FileSystemItem(string path, bool loadChildren=true)
        {
            //hacky: trailing slash won't come through, but is required to access root of drive
            if (path.EndsWith(":"))
            {
                path += @"\";
            }

            Path = path;

            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                _typeEnum = FileSystemItemType.Directory;

                DirectoryInfo dir = new DirectoryInfo(path);
                LastModifiedTime = dir.LastWriteTime;

                if (loadChildren)
                {
                    Children = dir.GetFileSystemInfos().Select(s => new FileSystemItem(s.FullName, false)).ToList<FileSystemItem>();
                }
            }
            else
            {
                _typeEnum = FileSystemItemType.File;

                FileInfo file = new FileInfo(path);
                Size = file.Length;
                LastModifiedTime = file.LastWriteTime;
            }
        }

    }
}
