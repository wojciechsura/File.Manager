using File.Manager.BusinessLogic.Models.Dialogs.FtpCredentials;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Base.Validation;
using File.Manager.BusinessLogic.ViewModels.Base.Validation.Builder;
using Spooksoft.VisualStateManager.Commands;
using Spooksoft.VisualStateManager.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.FtpCredentialsWindow
{
    public class FtpCredentialsWindowViewModel : BaseValidateableViewModel<FtpCredentialsWindowViewModel>
    {
        private readonly IFtpCredentialsWindowAccess access;

        private string username;
        private SecureString password;

        private void DoOk()
        {
            access.Close(true);
        }

        private void DoCancel()
        {
            access.Close(false);
        }

        public FtpCredentialsWindowViewModel(IFtpCredentialsWindowAccess access, string username) 
        {
            this.access = access;
            this.username = username;

            ValidationDefinitions = new ValidationRuleBuilder<FtpCredentialsWindowViewModel>().Build();

            var canConfirmCondition = !Condition.PropertyWatch(this, vm => vm.HasErrors, false);

            OkCommand = new AppCommand(obj => DoOk(), canConfirmCondition);
            CancelCommand = new AppCommand(obj => DoCancel());
        }

        protected override Dictionary<string, ValidationDefinition<FtpCredentialsWindowViewModel>> ValidationDefinitions { get; }

        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }

        public void SetPassword(SecureString value)
        {
            password = value;
        }

        public FtpCredentialsModel Result => new(username, password);

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
    }
}
