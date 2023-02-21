using Autofac;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.UserDecision;
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

namespace File.Manager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy UserDecisionDialog.xaml
    /// </summary>
    public partial class UserDecisionDialog : Window, IUserDecisionDialogAccess
    {
        private readonly UserDecisionDialogViewModel viewModel;

        public UserDecisionDialog(SingleProblemResolution[] availableResolutions, string header)
        {
            InitializeComponent();

            viewModel = Dependencies.Container.Instance.Resolve<UserDecisionDialogViewModel>(new NamedParameter("availableResolutions", availableResolutions),
                new NamedParameter("header", header),
                new NamedParameter("access", this));
            DataContext = viewModel;
        }

        public void Close(bool result)
        {
            DialogResult = result;
            Close();
        }

        public SingleProblemResolution Result => viewModel.Result;
    }
}
