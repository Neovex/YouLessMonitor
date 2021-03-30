using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouLessMonitor.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e)=> { };

        protected void UpdateProperty<T>(ref T oldVal, T newVal, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (String.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(nameof(propertyName));
            if ((oldVal != null && oldVal.Equals(newVal)) ||
                (oldVal == null && newVal == null)) return;

            oldVal = newVal;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void UpdateProperty<T>(string propertyName, Action<T> assignment, T oldVal, T newVal)
        {
            if (String.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(nameof(propertyName));
            if ((oldVal != null && oldVal.Equals(newVal)) ||
                (oldVal == null && newVal == null)) return;

            assignment.Invoke(newVal);
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}