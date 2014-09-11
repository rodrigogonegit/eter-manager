using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Services.Abstract
{
    interface IViewManager
    {
        void ShowWindow<T>(bool forceNewInsntance = false, string newTitle = null);

        void CloseWindow<T>();

        void HideWindow<T>();

        void RenameWindow<T>(string newName, string originalName = null);
    }
}
