using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Paths
{
    internal class PathService : IPathService
    {
        private const string PUBLISHER = "Publisher";
        private const string APPNAME = "Application";

        private readonly string appDataPath;

        public PathService()
        {
            appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PUBLISHER, APPNAME);
            Directory.CreateDirectory(appDataPath);
        }

        public string AppDataPath => appDataPath;
    }
}
