using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EterManager.Services;
using System.Windows;

namespace EterManager.Utilities
{
    public class UserInput
    {
        private static ILocale Locale = ((App)Application.Current).GetInstance<ILocale>();

        public static void ShowMessage(string msg, params object[] args)
        {
            System.Windows.Forms.MessageBox.Show(Locale.GetString(msg));
        }

        public static void YesNoMessage(string msg, params object[] args)
        {
            // ReSharper disable once LocalizableElement
            System.Windows.Forms.MessageBox.Show(msg, "Confirm operation", System.Windows.Forms.MessageBoxButtons.YesNo);
        }
    }
}
