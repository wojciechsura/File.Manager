using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File.Manager.BusinessLogic.Models.Files
{
    public abstract class FileListColumn
    {
        private int width;
        private FileListColumnWidthKind widthKind;

        private void SetWidth(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            width = value;
            OnWidthChanged();
        }

        private void SetWidthKind(FileListColumnWidthKind value)
        {
            if (value != widthKind)
            {
                widthKind = value;
                OnWidthChanged();
            }
        }

        protected void OnWidthChanged()
        {
            WidthChanged?.Invoke(this, EventArgs.Empty);    
        }

        protected FileListColumn()
        {
            width = 100;
            widthKind = FileListColumnWidthKind.Dip;
        }

        public int Width
        {
            get => width;
            set => SetWidth(value);
        }

        public FileListColumnWidthKind WidthKind
        {
            get => widthKind;
            set => SetWidthKind(value);
        }

        public event EventHandler WidthChanged;
    }
}
