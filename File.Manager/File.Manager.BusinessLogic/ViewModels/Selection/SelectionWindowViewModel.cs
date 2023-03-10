using File.Manager.BusinessLogic.Models.Dialogs.Selection;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.Resources.Windows.Selection;
using Spooksoft.VisualStateManager.Commands;
using Spooksoft.VisualStateManager.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.Selection
{
    public class SelectionWindowViewModel : BaseViewModel
    {
        private readonly ISelectionWindowAccess access;
        private readonly SelectionOperationKind operationKind;
        private SelectionMethod selectionMethod;
        private string mask;
        private string regularExpression;

        private readonly char[] invalidChars;

        private void DoCancel()
        {
            access.Close(false);
        }

        private void DoOk()
        {
            access.Close(true);
        }

        private bool ValidRegularExpression(string regularExpression)
        {
            try
            {
                _ = new Regex(regularExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidMask(string mask)
        {
            return (!string.IsNullOrEmpty(mask)) && !mask.Any(c => invalidChars.Contains(c));            
        }

        public SelectionWindowViewModel(ISelectionWindowAccess access, SelectionOperationKind operationKind) 
        {
            this.access = access;
            this.operationKind = operationKind;
            selectionMethod = SelectionMethod.Mask;

            mask = "*.*";
            regularExpression = "^.*$";

            invalidChars = System.IO.Path.GetInvalidFileNameChars()
                .Except(new[] { '*', '?' })
                .ToArray();

            var canConfirmCondition = Condition.ChainedLambda(this, vm => (SelectionMethod == SelectionMethod.Mask && ValidMask(mask)) || (SelectionMethod == SelectionMethod.RegularExpression && ValidRegularExpression(RegularExpression)), false);

            OkCommand = new AppCommand(obj => DoOk(), canConfirmCondition);
            CancelCommand = new AppCommand(obj => DoCancel());
        }

        public SelectionOperationKind OperationKind => operationKind;

        public SelectionMethod SelectionMethod
        {
            get => selectionMethod;
            set => Set(ref selectionMethod, value);
        }

        public string Mask
        {
            get => mask;
            set => Set(ref mask, value);
        }

        public string RegularExpression
        {
            get => regularExpression;
            set => Set(ref regularExpression, value);
        }

        public string Title => OperationKind switch
        {
            SelectionOperationKind.Add => Strings.Title_AddToSelection,
            SelectionOperationKind.Remove => Strings.Title_RemoveFromSelection,
            _ => throw new InvalidOperationException("Unsupported selection operation kind!")
        };

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public SelectionResultModel Result => new SelectionResultModel(SelectionMethod, Mask, RegularExpression);
    }
}
