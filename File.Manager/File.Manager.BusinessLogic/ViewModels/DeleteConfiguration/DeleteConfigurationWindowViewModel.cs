using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using File.Manager.Resources.Windows.DeleteConfigurationWindow;
using SmartFormat;
using File.Manager.BusinessLogic.Models.Dialogs.DeleteConfiguration;
using File.Manager.BusinessLogic.ViewModels.Base;
using System.Windows.Input;
using Spooksoft.VisualStateManager.Commands;
using File.Manager.API.Filesystem.Models.Items.Listing;
using Spooksoft.VisualStateManager.Conditions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace File.Manager.BusinessLogic.ViewModels.DeleteConfiguration
{
    public class DeleteConfigurationWindowViewModel : BaseViewModel
    {
        // Private fields -----------------------------------------------------

        private readonly IDeleteConfigurationWindowAccess access;

        private IReadOnlyList<Item> selectedItems;
        private string fileMask;

        // Private methods ----------------------------------------------------

        private void DoOk()
        {
            Result = new DeleteConfigurationModel(FileMask);
            access.Close(true);
        }

        private void DoCancel()
        {
            Result = null;
            access.Close(false);
        }        

        // Public methods -----------------------------------------------------

        public DeleteConfigurationWindowViewModel(DeleteConfigurationInputModel input, IDeleteConfigurationWindowAccess access)
        {
            this.access = access ?? throw new ArgumentNullException(nameof(access));

            this.selectedItems = input.SelectedItems;
            this.Address = input.Address;

            OkCommand = new AppCommand(obj => DoOk());
            CancelCommand = new AppCommand(obj => DoCancel());
        }

        // Public properties --------------------------------------------------

        public string SummaryHeader => Smart.Format(Strings.SummaryHeader_Delete, selectedItems.Count);

        public string SummaryValue => selectedItems.Count switch
        {
            1 => selectedItems.Single().Name,
            > 1 => Smart.Format(Strings.SummaryValue_Plural, selectedItems.Count.ToString()),
            _ => throw new InvalidOperationException("Invalid count of items to process!")
        };

        public string Address { get; }

        public string FileMask
        {
            get => fileMask;
            set => Set(ref fileMask, value);
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public DeleteConfigurationModel Result { get; private set; }
    }
}
