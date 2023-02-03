using Autofac;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.BusinessLogic.Services.Messaging;
using File.Manager.Services.DialogService;
using File.Manager.Services.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Dependencies
{
    public static class Configuration
    {
        private static bool configured = false;

        public static void Configure(ContainerBuilder builder)
        {
            if (configured)
                return;

            builder.RegisterType<MessagingService>().As<IMessagingService>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();

            File.Manager.BusinessLogic.Dependencies.Configuration.Configure(builder);

            configured = true;
        }
    }
}
