using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EterManager.Base;
using ObservableImmutable;

namespace EterManager.UserInterface.ViewModels
{
    class ExceptionWindowViewModel : ViewModelBase
    {
        public ExceptionWindowViewModel()
        {
            SystemInfoItems = new ObservableImmutableDictionary<string, string>();

            var type = typeof(Environment);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var pi in properties)
            {
                SystemInfoItems.Add(pi.Name, pi.GetValue(null).ToString());
            }
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

        private ObservableImmutableDictionary<string, string> _systemInfoItems;

        /// <summary>
        /// Gets or sets the system information items.
        /// </summary>
        /// <value>
        /// The system information items.
        /// </value>
        public ObservableImmutableDictionary<string, string> SystemInfoItems
        {
            get { return _systemInfoItems; }
            set { SetProperty(ref _systemInfoItems, value, "SystemInfoItems"); }
        }

    }
}
