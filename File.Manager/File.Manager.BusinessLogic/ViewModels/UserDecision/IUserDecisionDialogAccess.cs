using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.UserDecision
{
    public interface IUserDecisionDialogAccess
    {
        void Close(bool result);
    }
}
