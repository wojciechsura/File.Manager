using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Base;
using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.UserDecision
{
    public class UserDecisionDialogViewModel : BaseViewModel
    {
        private readonly IUserDecisionDialogAccess access;

        private void DoSelectResolution(SingleProblemResolution resolution)
        {
            Result = resolution;
            access.Close(true);
        }

        public UserDecisionDialogViewModel(SingleProblemResolution[] availableResolutions,
            string header,
            IUserDecisionDialogAccess access)
        {
            this.access = access;
            this.Header = header;

            AvailableResolutions = availableResolutions;

            SelectResolutionCommand = new AppCommand(obj => DoSelectResolution((SingleProblemResolution)obj));
        }

        public string Header { get; }

        public ICommand SelectResolutionCommand { get; }

        public SingleProblemResolution[] AvailableResolutions { get; }

        public SingleProblemResolution Result { get; private set; }
    }
}
