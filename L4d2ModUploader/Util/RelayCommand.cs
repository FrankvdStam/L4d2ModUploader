using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace L4d2ModUploader.Util
{
    public class RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null) : ICommand 
    {
        private Action<object?> _execute = execute;
        private Func<object?, bool>? _canExecute = canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
