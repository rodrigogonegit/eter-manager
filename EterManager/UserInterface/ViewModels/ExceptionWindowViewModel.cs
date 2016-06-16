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
            set { SetProperty(ref _exception, value, "Exception"); }
        }

    }
}
