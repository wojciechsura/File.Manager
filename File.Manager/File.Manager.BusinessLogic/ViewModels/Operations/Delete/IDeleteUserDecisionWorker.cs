using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations.Delete
{
    internal interface IDeleteUserDecisionWorker
    {
        SemaphoreSlim UserDecisionSemaphore { get; }

        SingleDeleteProblemResolution? UserDecision { get; set; }

    }
}
