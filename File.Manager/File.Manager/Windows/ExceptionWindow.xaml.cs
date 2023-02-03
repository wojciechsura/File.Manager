using File.Manager.BusinessLogic.ViewModels.ExceptionWindow;
using System;
using System.Collections.Generic;
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

namespace File.Manager
{
    /// <summary>
    /// Logika interakcji dla klasy ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window, IExceptionWindowAccess
    {
        private ExceptionWindowViewModel viewModel;

        public ExceptionWindow(Exception e)
        {
            InitializeComponent();

            viewModel = new ExceptionWindowViewModel(this, e);
            DataContext = viewModel;
        }
    }
}
