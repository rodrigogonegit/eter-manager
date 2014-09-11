using System;
using System.Linq;
using System.Windows;
using EterManager.Services.Abstract;

namespace EterManager.Services.Concrete
{
    class ViewManager : IViewManager
    {
        /// <summary>
        /// Shows window of designated type
        /// </summary>
        /// <typeparam name="T">Type of window to show</typeparam>
        /// <param name="forceNewInstance">Force new instance of window</param>
        public void ShowWindow<T>(bool forceNewInstance = false, string newTitle = null)
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault();
            if (window != null && forceNewInstance == false)
            {
                var wnd = (window as Window);
                wnd.Visibility = Visibility.Visible;
                wnd.WindowState = WindowState.Normal;
                wnd.Focus();

                if (newTitle != null)
                    wnd.Title = newTitle;
            }
            else
            {
                Window wnd = (Activator.CreateInstance<T>() as Window);
                wnd.Show();
                if (newTitle != null)
                    wnd.Title = newTitle;
            }
        }

        /// <summary>
        /// Closes Window
        /// </summary>
        /// <typeparam name="T">Type of window to close</typeparam>
        public void CloseWindow<T>()
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault();

            if (window != null)
                (window as Window).Close();
        }

        /// <summary>
        /// Hides Window
        /// </summary>
        /// <typeparam name="T">type of window to hide</typeparam>
        public void HideWindow<T>()
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault();

            if (window != null)
                (window as Window).Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Renames window
        /// </summary>
        /// <typeparam name="T">Type of window to be renamed</typeparam>
        /// <param name="newName">New name</param>
        /// <param name="originalName">Optional original name (in case there are several windows with the same type)</param>
        public void RenameWindow<T>(string newName, string originalName = null)
        {
            var windows = Application.Current.Windows.OfType<T>();

            if (windows != null)
            {
                if (originalName != null && originalName.Trim() != "")
                {
                    Window wnd = windows.Cast<Window>().FirstOrDefault(x => x.Title.Contains(originalName));

                    if (wnd != null)
                        wnd.Title = newName;
                }
                else
                {
                    Window wnd = windows.Cast<Window>().FirstOrDefault();

                    if (wnd != null)
                        wnd.Title = newName;
                }
            }
        }
    }
}
