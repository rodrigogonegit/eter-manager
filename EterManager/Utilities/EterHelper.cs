using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using EterManager.UserInterface.ViewModels;

namespace EterManager.Utilities
{
    public static class EterHelper
    {
        public static ClientProfileVM SelectedProfile { get; set; }

        /// <summary>
        /// Replaces .IndexFileExtension from string with .PackFileExtension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReplaceWithEpkExt(string file)
        {
            return file.Replace(SelectedProfile.IndexExtension, SelectedProfile.PackExtension);
        }

        /// <summary>
        /// Replaces .PackFileExtension from string with .IndexFileExtension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReplaceWithEixExt(string file)
        {
            return file.Replace(SelectedProfile.PackExtension, SelectedProfile.IndexExtension);
        }
    }
}
