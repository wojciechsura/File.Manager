using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.Operations.CopyMove;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public class CopyMoveProgressWindowViewModel : BaseViewModel
    {
        private readonly ICopyMoveProgressWindowAccess access;
        private readonly BaseCopyMoveOperationViewModel operation;

        private void HandleOperationFinished(object sender, EventArgs args)
        {
            access.Close();
        }

        public CopyMoveProgressWindowViewModel(BaseCopyMoveOperationViewModel operation, ICopyMoveProgressWindowAccess access)
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
