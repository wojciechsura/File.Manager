using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.Base
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Protected methods --------------------------------------------------

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null, Action changeHandler = null, bool force = false)
        {
            if (!Equals(field, value) || force)
            {
                field = value;
                OnPropertyChanged(propertyName);
                changeHandler?.Invoke();
            }
        }

        // Public properties --------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
