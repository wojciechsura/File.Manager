using Autofac;
using File.Manager.BusinessLogic.Services.Dialogs;
using File.Manager.Dependencies;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace File.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IDialogService dialogService;

        private void HandleUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            dialogService.ShowExceptionDialog(e.Exception);
            e.Handled = true;
        }

        public App()
        {
            Container.BuildContainer(File.Manager.Dependencies.Configuration.Configure);

            dialogService = File.Manager.Dependencies.Container.Instance.Resolve<IDialogService>();
            DispatcherUnhandledException += HandleUnhandledException;
        }
    }
}
