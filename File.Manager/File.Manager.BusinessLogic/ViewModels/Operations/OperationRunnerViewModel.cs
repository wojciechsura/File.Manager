using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public class OperationRunnerViewModel : BaseViewModel
    {
        private readonly IOperationRunnerWindowAccess access;
        private BaseOperationViewModel operation;

        private void HandleOperationFinished(object sender, EventArgs args)
        {
            access.Close();
        }

        public OperationRunnerViewModel(BaseOperationViewModel operation, IOperationRunnerWindowAccess access)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.access = access;

            operation.Finished += HandleOperationFinished;
        }

        public void NotifyLoaded()
        {
            operation.Run();
        }

        public BaseOperationViewModel Operation => operation;
    }
}
