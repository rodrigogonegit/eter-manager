using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EterManager.Utilities
{
    public class UserInput
    {
        public static void ShowMessage(string msg, params object[] args)
        {
            MessageBox.Show(msg);
        }

        public static void YesNoMessage(string msg, params object[] args)
        {
            // ReSharper disable once LocalizableElement
            MessageBox.Show(msg, "Confirm operation", MessageBoxButtons.YesNo);
        }
    }
}
