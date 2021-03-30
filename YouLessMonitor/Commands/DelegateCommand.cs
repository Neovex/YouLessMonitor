using System;

namespace YouLessMonitor.Commands
{
    public class DelegateCommand : BaseCommand<object>
    {
        private Func<bool> _CanExecute;
        private Action _Execute;
        private string _Name;


        public DelegateCommand(Action execute, Func<bool> canExecute = null, string name = null)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute ?? (() => true);
            _Name = name;
        }

        protected override bool CanExecuteInternal(object p)
        {
            var r = _CanExecute.Invoke();
            if (_Name != null) Log.Debug(_Name, r);
            return r;
        }
        protected override void ExecuteInternal(object p)
        {
            if (_Name != null) Log.Debug(_Name);
            _Execute.Invoke();
        }
    }

    public class DelegateCommand<T> : BaseCommand<T>
    {
        private Func<T, bool> _CanExecute;
        private Action<T> _Execute;
        private string _Name;


        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null, string name = null)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute ?? (p => true);
            _Name = name;
        }

        protected override bool CanExecuteInternal(T p)
        {
            var r = _CanExecute.Invoke(p);
            if (_Name != null) Log.Debug(_Name, r, p);
            return r;
        }
        protected override void ExecuteInternal(T p)
        {
            if (_Name != null) Log.Debug(_Name, p);
            _Execute.Invoke(p);
        }
    }
}