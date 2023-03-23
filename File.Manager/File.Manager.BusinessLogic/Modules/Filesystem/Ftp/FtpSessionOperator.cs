using File.Manager.API.Filesystem;
using File.Manager.API.Filesystem.Models.Items.Listing;
using File.Manager.API.Filesystem.Models.Items.Operator;
using File.Manager.API.Filesystem.Models.Items.Plan;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public class FtpSessionOperator : FilesystemOperator
    {
        private readonly IConfigurationService configurationService;

        public FtpSessionOperator(IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        public override OperationPlan BuildOperationPlanFromSelection(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            throw new NotSupportedException();
        }

        public override bool? CheckIsSubfolderEmpty(string name)
        {
            throw new NotSupportedException();
        }

        public override bool CreateFolder(string name)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteEmptyFolder(string name)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteFile(string name)
        {
            var session = configurationService.Configuration.Ftp.Sessions.FirstOrDefault(s => s.SessionName.Value.ToLowerInvariant() == name.ToLowerInvariant());
            if (session == null)
                return false;

            configurationService.Configuration.Ftp.Sessions.Remove(session);
            configurationService.Save();

            return true;
        }

        public override void Dispose()
        {
            
        }

        public override FilesystemOperator EnterFolder(string name)
        {
            throw new NotSupportedException();
        }

        public override bool? FileExists(string name)
        {
            var session = configurationService.Configuration.Ftp.Sessions.FirstOrDefault(s => s.SessionName.Value.ToLowerInvariant() == name.ToLowerInvariant());
            return session != null;
        }

        public override bool? FolderExists(string name)
        {
            return false;
        }

        public override FileAttributes? GetFileAttributes(string targetName)
        {
            return (FileAttributes)0;
        }

        public override IReadOnlyList<BaseOperatorItem> List(IReadOnlyList<Item> selectedItems, string fileMaskOverride)
        {
            var result = new List<BaseOperatorItem>();

            foreach (var session in configurationService.Configuration.Ftp.Sessions)
            {
                if (selectedItems != null && !selectedItems.Any(si => si.Name.ToLowerInvariant() == session.SessionName.Value.ToLowerInvariant()))
                    continue;

                if (!PatternMatcher.StrictMatchPattern(fileMaskOverride, session.SessionName.Value))
                    continue;

                result.Add(new OperatorFileItem(session.SessionName.Value, 0, false, false, false));
            }

            return result;
        }

        public override Stream OpenFileForReading(string name)
        {
            throw new NotSupportedException();
        }

        public override Stream OpenFileForWriting(string name)
        {
            throw new NotSupportedException();
        }

        public override bool SetFileAttributes(string targetName, FileAttributes attributes)
        {
            return true;
        }

        public override string CurrentPath => FtpSessionsNavigator.ROOT_ADDRESS;
    }
}
