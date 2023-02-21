using File.Manager.BusinessLogic.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Operations
{
    public class CopyMoveDescriptionViewModel : BaseViewModel 
    {
        private string fromAddress;
        private string toAddress;
        private string state;

        public string FromAddress
        {
            get => fromAddress; 
            set => Set(ref fromAddress, value);
        }

        public string ToAddress
        {
            get => toAddress;
            set => Set(ref toAddress, value);
        }
        
        public string State
        {
            get => state;
            set => Set(ref state, value);
        }
    }
}
