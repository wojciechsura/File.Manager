using File.Manager.BusinessLogic.Models.Configuration;
using File.Manager.BusinessLogic.Services.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IPathService pathService;
        private readonly ConfigModel config;

        public ConfigurationService(IPathService pathService)
        {
            this.pathService = pathService;
            this.config = new ConfigModel();

            var configPath = pathService.ConfigPath;
            try
            {
                config.Load(configPath);
            }
            catch
            {
                config.SetDefaults();
            }
        }

        public bool Save()
        {
            try
            {
                config.Save(pathService.ConfigPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConfigModel Configuration => config;
    }
}
