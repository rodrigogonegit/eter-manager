using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using EterManager.Models;
using System.Threading.Tasks;

namespace EterManager.Services.Abstract
{
    public interface IAppUpdater
    {
        /// <summary>
        /// Checks the version
        /// </summary>
        /// <returns></returns>
        Task CheckVersions(Type targetSubscriber = null);

        /// <summary>
        /// Downloads the version package.
        /// </summary>
        Task DownloadLatestVersion();

        /// <summary>
        /// Installs the latest version.
        /// </summary>
        void InstallLatestVersion();

        /// <summary>
        /// Determines whether the latest version correspondent update package is available (downloaded).
        /// </summary>
        /// <returns></returns>
        bool IsUpdatePackageAvailable();

        /// <summary>
        /// Found new version
        /// </summary>
        event CheckVersionHandler CheckVersionsCompleted;

        /// <summary>
        /// Occurs when [download progress changed].
        /// </summary>
        event DownloadProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Occurs when [download completed].
        /// </summary>
        event AsyncCompletedEventHandler DownloadCompleted;

        /// <summary>
        /// Gets or sets the latest version.
        /// </summary>
        /// <value>
        /// The latest version.
        /// </value>
        VersionModel LatestVersion { get; set; }

        /// <summary>
        /// Indicates if an update is available
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is update available; otherwise, <c>false</c>.
        /// </value>
        bool IsUpdateAvailable { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CheckVersionEventArgs : EventArgs
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="hasNewVersionAvailable">Defines wether a new version is available</param>
        /// <param name="targetSubscriberType">Sets the type of the subscriber to be executed</param>
        public CheckVersionEventArgs(bool hasNewVersionAvailable, Type targetSubscriberType)
        {
            HasNewVersionAvailable = hasNewVersionAvailable;
            AskToDownloadHandled = false;
            TargetSubscriberType = targetSubscriberType;
        }

        /// <summary>
        /// Defines wether a new version is available
        /// </summary>
        public bool HasNewVersionAvailable { get; private set; }

        /// <summary>
        /// Defines wether the subscriber should or shouldn't ask to download the update
        /// </summary>
        public bool AskToDownloadHandled { get; set; }

        /// <summary>
        /// Sets the type of the subscriber to be executed
        /// Intended to be used when the type of the subscriber we want to be executed is known before-hand
        /// </summary>
        public Type TargetSubscriberType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CheckVersionHandler(object sender, CheckVersionEventArgs e);
}