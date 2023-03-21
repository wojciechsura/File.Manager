using Autofac;
using Dev.Editor.BusinessLogic.Services.Highlighting;
using File.Manager.API;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.Services.EventBus;
using File.Manager.BusinessLogic.Services.Host;
using File.Manager.BusinessLogic.Services.Icons;
using File.Manager.BusinessLogic.Services.Modules;
using File.Manager.BusinessLogic.Services.Paths;
using File.Manager.BusinessLogic.Services.View;
using File.Manager.BusinessLogic.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Dependencies
{
    public static class Configuration
    {
        private static bool isConfigured = false;

        public static void Configure(ContainerBuilder builder)
        {
            if (isConfigured)
                return;
            isConfigured = true;

            // Register services
            builder.RegisterType<EventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<PathService>().As<IPathService>().SingleInstance();
            builder.RegisterType<ModuleService>().As<IModuleService>().SingleInstance();
            builder.RegisterType<ModuleHost>().As<IModuleHost>().SingleInstance();
            builder.RegisterType<IconService>().As<IIconService>().SingleInstance();
            builder.RegisterType<HighlightingProvider>().As<IHighlightingProvider>().SingleInstance();
            builder.RegisterType<DisplayKindResolver>().As<IDisplayKindResolver>().SingleInstance();
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();
        }
    }
}
