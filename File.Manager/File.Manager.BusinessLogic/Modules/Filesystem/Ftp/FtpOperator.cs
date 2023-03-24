using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.Common.Helpers;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpOperator : FilesystemOperator
    {
        private readonly FtpClient ftpClient;
        private readonly (string host, string username) sessionKey;
        private readonly string workingDirectory;

        public FtpOperator(FtpClient ftpClient, string workingDirectory, (string host, string username) sessionKey)
        {
            this.ftpClient = ftpClient;
            this.sessionKey = sessionKey;
            this.workingDirectory = PathHelper.EnsureTrailingSlash(workingDirectory);
        }

        public override OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
            => throw new NotSupportedException();        

        public override bool? CheckIsSubfolderEmpty(string name)
        {
            try
            {
                string folderPath = $"{workingDirectory}{name}/";
                return InternalCheckSubfolderEmpty(folderPath);
            }
            catch
            {
                return null;
            }
        }

        private bool InternalCheckSubfolderEmpty(string folderPath)
        {
            return !ftpClient.GetListing(folderPath).Any(i => i.Type == FtpObjectType.File || (i.Type == FtpObjectType.Directory && i.SubType == FtpObjectSubType.SubDirectory) || (i.Type == FtpObjectType.Link));
        }

        private void EnsureWorkingDirectory()
        {
            if (ftpClient.GetWorkingDirectory() != workingDirectory)
                ftpClient.SetWorkingDirectory(workingDirectory);
        }

        public override bool CreateFolder(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                ftpClient.CreateDirectory(name);
                return true;
            }
            catch
            {
                return false;
            }            
        }

        public override bool DeleteEmptyFolder(string name)
        {
            try
            {
                string folderPath = $"{workingDirectory}{name}/";
                var empty = InternalCheckSubfolderEmpty(folderPath);
                if (!empty)
                    return false;

                EnsureWorkingDirectory();
                ftpClient.DeleteDirectory(name);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool DeleteFile(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                ftpClient.DeleteFile(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Dispose()
        {
            // DO NOT close the client. Client is shared between
            // FtpNavigator and other FtpOperators. It will be
            // disposed of by the navigator when user leaves the
            // FTP root folder.
        }

        public override FilesystemOperator EnterFolder(string name)
        {
            string newPath = $"{workingDirectory}{name}/";
            return new FtpOperator(ftpClient, newPath, sessionKey);
        }

        public override bool? FileExists(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                return ftpClient.FileExists(name);
            }
            catch
            {
                return null;
            }
        }

        public override bool? FolderExists(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                return ftpClient.DirectoryExists(name);
            }
            catch
            {
                return null;
            }
        }

        public override FileAttributes? GetFileAttributes(string targetName)
        {
            // TODO

            return (FileAttributes)0;
        }

        public override IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            try
            {
                EnsureWorkingDirectory();
                var list = ftpClient.GetListing();

                var result = new List<BaseOperatorItem>();

                foreach (var folder in list.Where(e => e.Type == FtpObjectType.Directory && e.SubType == FtpObjectSubType.SubDirectory))
                {
                    // FTP is case sensitive
                    if (selectedItems != null && !selectedItems.Any(i => i.Name == folder.Name))
                        continue;

                    if (!PatternMatcher.StrictMatchPattern(fileMaskOverride, folder.Name))
                        continue;

                    var folderItem = new OperatorFolderItem(folder.Name);
                    result.Add(folderItem);
                }

                foreach (var file in list.Where(e => e.Type is FtpObjectType.File or FtpObjectType.Link))
                {
                    if (selectedItems != null && !selectedItems.Any(i => i.Name == file.Name))
                        continue;

                    if (!PatternMatcher.StrictMatchPattern(fileMaskOverride, file.Name))
                        continue;

                    // TODO attributes
                    var fileItem = new OperatorFileItem(file.Name, file.Size, false, false, false);
                    result.Add(fileItem);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public override Stream OpenFileForReading(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                return ftpClient.OpenRead(name);
            }
            catch
            {
                return null;
            }
        }

        public override Stream OpenFileForWriting(string name)
        {
            try
            {
                EnsureWorkingDirectory();
                return ftpClient.OpenWrite(name);
            }
            catch
            {
                return null;
            }
        }

        public override bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            // TODO
            return true;
        }

        public override string CurrentPath => $"{FtpSessionsNavigator.ROOT_ADDRESS}{sessionKey.username}@{sessionKey.host}>{workingDirectory}";
    }
}
