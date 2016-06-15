using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using EterManager.Models;

namespace EterManager.Services.Abstract
{
    public interface IAppUpdater
    {
        /// <summary>
        /// Checks the version
        /// </summary>
        /// <returns></returns>
        void CheckVersions();

        /// <summary>
        /// Downloads the version package.
        /// </summary>
        void DownloadLatestVersion();

        /// <summary>
        /// Installs the latest version.
        /// </summary>
        void InstallLatestVersion();

        /// <summary>
        /// Found new version
        /// </summary>
        event EventHandler NewVersionFound;

        /// <summary>
        /// Occurs when [download progress changed].
        /// </summary>
        event DownloadProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Occurs when [download completed].
        /// </summary>
        event AsyncCompletedEventHandler DownloadCompleted;
    }
}