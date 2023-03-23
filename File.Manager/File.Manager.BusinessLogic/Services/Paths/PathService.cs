using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Paths
{
    internal class PathService : IPathService
    {
        private const string PUBLISHER = "Spooksoft";
        private const string APPNAME = "File.Manager";

        private const string CONFIG_FILENAME = "Config.xml";

        private readonly string appDataPath;
        private readonly string configPath;
        private readonly string appExecutablePath;

        public PathService()
        {
            appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PUBLISHER, APPNAME);
            Directory.CreateDirectory(appDataPath);

            configPath = Path.Combine(appDataPath, CONFIG_FILENAME);

            var executingAssembly = Assembly.GetEntryAssembly();
            appExecutablePath = new Uri(executingAssembly.Location).LocalPath;
        }

        public string AppDataPath => appDataPath;

        public string ConfigPath => configPath;
    }
}
