using File.Manager.BusinessLogic.Types;

namespace File.Manager.BusinessLogic.Models.Dialogs.Selection
{
    public class SelectionResultModel
    {
        public SelectionResultModel(SelectionMethod selectionMethod, string mask, string regularExpression)
        {
            this.SelectionMethod = selectionMethod;
            this.Mask = mask;
            this.RegularExpression = regularExpression;
        }

        public SelectionMethod SelectionMethod { get; }

        public string Mask { get; }

        public string RegularExpression { get; }
    }
}