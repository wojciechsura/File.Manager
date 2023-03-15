using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipOperator : IFilesystemOperator
    {
        public string CurrentPath => throw new NotImplementedException();

        public OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            throw new NotImplementedException();
        }

        public bool? CheckIsSubfolderEmpty(string name)
        {
            throw new NotImplementedException();
        }

        public bool CreateFolder(string name)
        {
            throw new NotImplementedException();
        }

        public bool DeleteFile(string name)
        {
            throw new NotImplementedException();
        }

        public bool DeleteFolder(string name, bool onlyIfEmpty)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IFilesystemOperator EnterFolder(string name)
        {
            throw new NotImplementedException();
        }

        public bool? FileExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool? FolderExists(string name)
        {
            throw new NotImplementedException();
        }

        public FileAttributes? GetFileAttributes(string targetName)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            throw new NotImplementedException();
        }

        public Stream OpenFileForReading(string name)
        {
            throw new NotImplementedException();
        }

        public Stream OpenFileForWriting(string name)
        {
            throw new NotImplementedException();
        }

        public bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            throw new NotImplementedException();
        }
    }
}
