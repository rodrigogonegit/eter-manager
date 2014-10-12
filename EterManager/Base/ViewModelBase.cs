using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using EterManager.Services.Abstract;
using EterManager.Services;

namespace EterManager.Base
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        internal IViewManager ViewManager = ((App)Application.Current).GetInstance<IViewManager>();
        internal ILogger Logger = ((App)Application.Current).GetInstance<ILogger>();
        internal ILocale Locale = ((App)Application.Current).GetInstance<ILocale>();
        internal IEventAggregator EventAggregator = ((App)Application.Current).GetInstance<IEventAggregator>();

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            //checkIfPropertyNameExists(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Property Set
        // Minor check, however in cuttting and pasting a new property we can make errors that pass this test
        // Oops, forget where I found these.. Suppose Sacha Barber introduced this test or at least blogged about it
        [Conditional("DEBUG")]
        private void _CheckIfPropertyNameExists(String propertyName)
        {
            Type type = GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }

        // From codeproject article Performance-and-Ideas-from-Anders-Hejlsberg-INotify (long link; google)
        // Note that we can use a text utility that checks/corrects field/propName on string basis
        public bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                _CheckIfPropertyNameExists(propertyName);

                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}
