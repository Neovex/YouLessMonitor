using System;
using System.Windows.Input;

namespace YouLessMonitor.Commands
{
    public abstract class BaseCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged = (s, e) => { };

        public bool CanExecute(object parameter) => (parameter == null || parameter is T) && CanExecuteInternal((T)parameter);
        protected virtual bool CanExecuteInternal(T parameter) => true;

        public void Execute(object parameter) => ExecuteInternal((T)parameter);
        protected abstract void ExecuteInternal(T parameter);

        protected void InvokeCanExecuteChanged() => CanExecuteChanged.Invoke(this, EventArgs.Empty);
    }
}