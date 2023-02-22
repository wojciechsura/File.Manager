﻿using File.Manager.API.Filesystem.Models.Items;
using File.Manager.API.Filesystem.Models.Operator;
using File.Manager.API.Filesystem.Models.Plan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem
{
    public interface IFilesystemOperator : IDisposable
    {
        OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride);

        /// <summary>Creates folder with given name if one does not exist.</summary>
        bool CreateFolder(string name);

        /// <summary>Deletes file with given name.</summary>
        bool DeleteFile(string name);

        /// <summary>Deletes folder with given name.</summary>
        bool DeleteFolder(string name);

        /// <summary>Enters folder with given name in current location</summary>
        IFilesystemOperator EnterFolder(string name);

        /// <summary>Checks if given file exists</summary>
        bool FileExists(string name);

        /// <summary>Checks if given folder exists</summary>        
        bool FolderExists(string name);

        /// <summary>Returns existing file's attributes</summary>
        FileAttributes? GetFileAttributes(string targetName);

        /// <summary>Lists contents of the current location</summary>
        IReadOnlyList<BaseOperatorItem> List();

        /// <summary>Returns stream for existing file allowing reading.</summary>
        Stream OpenFileForReading(string name);

        /// <summary>Returns stream for new file for writing.</summary>
        Stream OpenFileForWriting(string name);

        bool SetFileAttributes(string targetName, FileAttributes attributes);

        string CurrentPath { get; }
    }
}
