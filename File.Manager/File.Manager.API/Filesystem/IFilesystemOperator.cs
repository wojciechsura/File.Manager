using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem
{
    public abstract class FilesystemOperator : IDisposable
    {
        public abstract OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string? fileMaskOverride);

        /// <summary>Creates folder with given name if one does not exist.</summary>
        public abstract bool CreateFolder(string name);

        /// <summary>Deletes file with given name.</summary>
        public abstract bool DeleteFile(string name);

        /// <summary>Deletes folder with given name.</summary>
        public abstract bool DeleteEmptyFolder(string name);

        public abstract void Dispose();

        /// <summary>Enters folder with given name in current location</summary>
        public abstract FilesystemOperator EnterFolder(string name);

        /// <summary>Checks if given file exists</summary>
        public abstract bool? FileExists(string name);

        /// <summary>Checks if given folder exists</summary>        
        public abstract bool? FolderExists(string name);

        /// <summary>Returns existing file's attributes</summary>
        public abstract FileAttributes? GetFileAttributes(string targetName);

        /// <summary>Lists contents of the current location</summary>
        public abstract IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item>? selectedItems, string? fileMaskOverride);

        /// <summary>Returns stream for existing file allowing reading.</summary>
        public abstract Stream OpenFileForReading(string name);

        public virtual void CloseReadFile(Stream stream, string name)
        {
            stream.Dispose();
        }

        /// <summary>Returns stream for new file for writing.</summary>
        public abstract Stream OpenFileForWriting(string name);

        public virtual void CloseWrittenFile(Stream stream, string name)
        {
            stream.Dispose();
        }

        public abstract bool SetFileAttributes(string targetName, FileAttributes attributes);

        public abstract bool? CheckIsSubfolderEmpty(string name);

        public abstract string CurrentPath { get; }
    }
}
