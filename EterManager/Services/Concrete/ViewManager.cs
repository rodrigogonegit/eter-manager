using System;
using System.Linq;
using System.Windows;
using EterManager.Services.Abstract;

namespace EterManager.Services.Concrete
{
    /// <summary>
    /// Manages all secondary view instances
    /// </summary>
    /// <seealso cref="EterManager.Services.Abstract.IViewManager" />
    class ViewManager : IViewManager
    {
        /// <summary>
        /// Shows window of designated type
        /// </summary>
        /// <typeparam name="T">Type of window to show</typeparam>
        /// <param name="forceNewInstance">Force new instance of window</param>
        /// <param name="newTitle"></param>
        /// <param name="titleMustMatch"></param>
        public Window ShowWindow<T>(bool forceNewInstance = false, string newTitle = null, bool titleMustMatch = false)
        {
            Window window = null;

            if (titleMustMatch)
            {
                foreach (var w in Application.Current.Windows.OfType<T>())
                {
                    if ((w as Window).Title == newTitle)
                        window = w as Window;
                }
            }
            else
            {
                window = Application.Current.Windows.OfType<T>().FirstOrDefault() as Window;
            }

            if (window != null && forceNewInstance == false)
            {
                var wnd = window;
                wnd.Visibility = Visibility.Visible;
                wnd.WindowState = WindowState.Normal;
                wnd.Focus();

                if (newTitle != null)
                    wnd.Title = newTitle;
            }
            else
            {
                var wnd = Activator.CreateInstance<T>() as Window;
                if (wnd == null) return null;
                wnd.Show();
                if (newTitle != null)
                    wnd.Title = newTitle;

                return wnd;
            }

            return window;
        }

        /// <summary>
        /// Closes Window
        /// </summary>
        /// <typeparam name="T">Type of window to close</typeparam>
        public void CloseWindow<T>(string matchingTitle = "")
        {
            var window = (matchingTitle == "")
                ? Application.Current.Windows.OfType<T>().FirstOrDefault() as Window
                : Application.Current.Windows.OfType<T>().Cast<Window>().FirstOrDefault(x => x.Title == matchingTitle);

            if (window != null)
                window.Close();
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
                    var wnd = windows.Cast<Window>().FirstOrDefault(x => x.Title.Contains(originalName));

                    if (wnd != null)
                        wnd.Title = newName;
                }
                else
                {
                    var wnd = windows.Cast<Window>().FirstOrDefault();

                    if (wnd != null)
                        wnd.Title = newName;
                }
            }
        }

        /// <summary>
        /// Terminates application instance
        /// </summary>
        public void TerminateProgram()
        {
            Application.Current.Shutdown();
        }
    }
}
