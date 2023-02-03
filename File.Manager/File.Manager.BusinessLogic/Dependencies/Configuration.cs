using Autofac;
using File.Manager.BusinessLogic.Services.EventBus;
using File.Manager.BusinessLogic.Services.Paths;
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

            // Register viewmodels
            builder.RegisterType<MainWindowViewModel>().WithParameter("access", null);
        }
    }
}
