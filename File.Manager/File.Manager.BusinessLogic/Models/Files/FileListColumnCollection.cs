using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Files
{
    public class FileListColumnCollection : ObservableCollection<FileListColumn>
    {
        // Private methods ----------------------------------------------------

        private void HandleColumnWidthsChanged(object sender, EventArgs e)
        {
            OnColumnWidthsChanged();
        }

        // Protected methods --------------------------------------------------

        protected void OnColumnWidthsChanged()
        {
            ColumnWidthsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void InsertItem(int index, FileListColumn item)
        {
            base.InsertItem(index, item);
            if (item != null)
                item.WidthChanged += HandleColumnWidthsChanged;
        }

        protected override void RemoveItem(int index) 
        {
            var item = this[index];
            if (item != null)
                item.WidthChanged -= HandleColumnWidthsChanged;

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, FileListColumn item)
        {
            var existing = this[index];
            if (existing != null)
                existing.WidthChanged -= HandleColumnWidthsChanged;

            base.SetItem(index, item);

            if (item != null)
                item.WidthChanged += HandleColumnWidthsChanged;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
                if (item != null)
                    item.WidthChanged -= HandleColumnWidthsChanged;

            base.ClearItems();
        }

        // Public properties --------------------------------------------------

        public event EventHandler ColumnWidthsChanged;
    }
}
