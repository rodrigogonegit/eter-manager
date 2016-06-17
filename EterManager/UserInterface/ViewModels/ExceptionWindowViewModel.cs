using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EterManager.Base;

namespace EterManager.UserInterface.ViewModels
{
    class ExceptionWindowViewModel : ViewModelBase
    {
        public ExceptionWindowViewModel()
        {
            
        }

        private Exception _exception;

        /// <summary>
        /// Exception object
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
            set
            {
                ExceptionStr = value.ToString();
                SetProperty(ref _exception, value, "Exception");
            }
        }

        private string _exceptionStr;

        /// <summary>
        /// Exception str
        /// </summary>
        public string ExceptionStr
        {
            get { return _exceptionStr; }
            set { SetProperty(ref _exceptionStr, value, "ExceptionStr"); }
        }
    }
}
