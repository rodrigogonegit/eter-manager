using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EterManager.Utilities
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        /// <summary>
        /// Constructor with null CanExecute value
        /// </summary>
        /// <param name="execute"></param>
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        /// <summary>
        /// Constructor overload which enables "CanExecute" capability
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// ICommand implementation
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion
    }
}
