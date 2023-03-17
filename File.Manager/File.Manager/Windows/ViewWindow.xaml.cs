using Autofac;
using File.Manager.BusinessLogic.ViewModels.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace File.Manager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window, IViewWindowAccess
    {
        private readonly ViewWindowViewModel viewModel;

        public ViewWindow(Stream stream, string filename)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<ViewWindowViewModel>(new NamedParameter("access", this),
                new NamedParameter("stream", stream),
                new NamedParameter("filename", filename));
            DataContext = viewModel;
        }
    }
}
