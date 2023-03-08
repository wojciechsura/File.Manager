using File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration;
using File.Manager.BusinessLogic.ViewModels.Base;
using Spooksoft.VisualStateManager.Commands;
using Spooksoft.VisualStateManager.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.NewFolderConfiguration
{
    public class NewFolderConfigurationWindowViewModel : BaseViewModel
    {
        private readonly INewFolderConfigurationWindowAccess access;
        private readonly char[] invalidChars;
        private string name;

        private void DoOk()
        {
            access.Close(true);
        }

        private void DoCancel()
        {
            access.Close(false);
        }

        private bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return !name.Any(c => invalidChars.Contains(c));
        }


        public NewFolderConfigurationWindowViewModel(INewFolderConfigurationWindowAccess access)
        {
            this.access = access;

            invalidChars = System.IO.Path.GetInvalidFileNameChars();

            var nameValidCondition = Condition.ChainedLambda(this, vm => ValidateName(vm.Name), false);
            var canConfirmCondition = nameValidCondition;

            OkCommand = new AppCommand(obj => DoOk(), canConfirmCondition);
            CancelCommand = new AppCommand(obj => DoCancel());
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }


        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public NewFolderConfigurationModel Result => new NewFolderConfigurationModel(name);
    }
}
