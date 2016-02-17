using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EterManager.Services.Abstract
{
    interface IViewManager
    {
        Window ShowWindow<T>(bool forceNewInsntance = false, string newTitle = null, bool titleMustMatch = false);

        void CloseWindow<T>(string matchingTitle = "");

        void HideWindow<T>();

        void RenameWindow<T>(string newName, string originalName = null);

        void TerminateProgram();
    }
}
