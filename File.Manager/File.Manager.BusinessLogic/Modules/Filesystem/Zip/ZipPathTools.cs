using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public static class ZipPathTools
    {
        public const string ROOT_ADDRESS = @"\\Zip\";

        public static (string location, string filename) GetZipEntryLocation(string zipEntryName, bool locationWithTrailingSlash = false)
        {
            if (zipEntryName.EndsWith('/'))
                zipEntryName = zipEntryName[..^1];

            int lastSlash = zipEntryName.LastIndexOf('/');

            return lastSlash switch
            {
                -1 => (string.Empty, zipEntryName),
                _ => (zipEntryName[0..(locationWithTrailingSlash ? lastSlash + 1 : lastSlash)], zipEntryName[(lastSlash + 1)..])
            };
        }

        public static string BuildAddress(string zipFilePath, string internalPath)
        {
            return $"{ROOT_ADDRESS}{zipFilePath}>{internalPath}";
        }
    }
}
