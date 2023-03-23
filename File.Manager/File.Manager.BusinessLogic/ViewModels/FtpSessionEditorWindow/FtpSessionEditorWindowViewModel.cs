using File.Manager.BusinessLogic.Attributes;
using File.Manager.BusinessLogic.Models.Configuration.Ftp;
using File.Manager.BusinessLogic.Services.Configuration;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Base.Validation;
using File.Manager.BusinessLogic.ViewModels.Base.Validation.Builder;
using File.Manager.Resources.Windows.FtpSessionEditor;
using Spooksoft.VisualStateManager.Commands;
using Spooksoft.VisualStateManager.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.FtpSessionEditorWindow
{
    public class FtpSessionEditorWindowViewModel : BaseValidateableViewModel<FtpSessionEditorWindowViewModel>
    {
        private static readonly Regex hostRegex = new Regex(@"[\w_\-]+(\.[\w_\-]+)+");

        private readonly IFtpSessionEditorWindowAccess access;
        private readonly IConfigurationService configurationService;
        private FtpSession model;
        private bool isEditing;

        [SyncWithModel(nameof(FtpSession.SessionName))]
        private string sessionName;
        [SyncWithModel(nameof(FtpSession.Host))]
        private string host;
        [SyncWithModel(nameof(FtpSession.Port))]
        private int port;
        [SyncWithModel(nameof(FtpSession.Username))]
        private string username;

        private void DoCancel()
        {
            access.Close(false);
        }

        private void DoOk()
        {
            if (!isEditing)
            {
                model = new FtpSession();
                configurationService.Configuration.Ftp.Sessions.Add(model);
            }

            UpdateToModel(model);
            configurationService.Save();

            access.Close(true);
        }

        private IEnumerable<string> ValidateSessionName(FtpSessionEditorWindowViewModel viewModel, string propertyName)
        {
            if (configurationService.Configuration.Ftp.Sessions
                .Except(new[] { model })
                .Any(s => s.SessionName.Value.ToLowerInvariant() == viewModel.SessionName.ToLowerInvariant()))
                yield return Strings.Error_SessionNameDuplicated;

            if (string.IsNullOrWhiteSpace(viewModel.SessionName))
                yield return Strings.Error_SessionNameEmpty;
        }

        private IEnumerable<string> ValidateHost(FtpSessionEditorWindowViewModel viewModel, string propertyName)
        {
            if (!hostRegex.IsMatch(viewModel.Host))
                yield return Strings.Error_InvalidHost;
        }

        private IEnumerable<string> ValidatePort(FtpSessionEditorWindowViewModel viewModel, string propertyName)
        {
            if (viewModel.Port < 0 || viewModel.Port > 65535)
                yield return Strings.Error_InvalidPort;
        }

        protected override Dictionary<string, ValidationDefinition<FtpSessionEditorWindowViewModel>> ValidationDefinitions { get; }

        public FtpSessionEditorWindowViewModel(IFtpSessionEditorWindowAccess access,
            IConfigurationService configurationService,
            FtpSession model)
        {
            this.access = access;
            this.configurationService = configurationService;

            if (model == null)
            {
                this.model = null;
                isEditing = false;
            }
            else
            {
                this.model = model;
                isEditing = true;

                UpdateFromModel(this.model);
            }

            ValidationDefinitions = new ValidationRuleBuilder<FtpSessionEditorWindowViewModel>()
                .For(vm => vm.SessionName, r => r.AddRule(ValidateSessionName))
                .For(vm => vm.Host, r => r.AddRule(ValidateHost))
                .For(vm => vm.Port, r => r.AddRule(ValidatePort))                
                .Build();

            var canConfirmCondition = !Condition.PropertyWatch(this, vm => vm.HasErrors, false);

            OkCommand = new AppCommand(obj => DoOk(), canConfirmCondition);
            CancelCommand = new AppCommand(obj => DoCancel());

        }

        public string SessionName
        {
            get => sessionName;
            set => Set(ref sessionName, value);
        }

        public string Host
        {
            get => host;
            set => Set(ref host, value);
        }

        public int Port
        {
            get => port; 
            set => Set(ref port, value);
        }

        public string Username
        {
            get => username; 
            set => Set(ref username, value);
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public string Title => isEditing ? Strings.Title_Edit : Strings.Title_New;

        public FtpSession Result => model;
    }
}
