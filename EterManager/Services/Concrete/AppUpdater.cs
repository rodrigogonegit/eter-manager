using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EterManager.Base;
using EterManager.Services.Abstract;
using Newtonsoft.Json;
using EterManager.Models;
using EterManager.Utilities;
using System.Timers;
using EterManager.UserInterface.ViewModels;

namespace EterManager.Services.Concrete
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="EterManager.Services.Abstract.IAppUpdater" />
    class AppUpdater : IAppUpdater
    {
        /// <summary>
        /// The _aux executable version (AppUpdater.exe)
        /// </summary>
        private string _appUpdaterHash;

        /// <summary>
        /// The _current version
        /// </summary>
        private Version _currentVersion;

        /// <summary>
        /// The _web client
        /// </summary>
        private readonly WebClient _webClient;

        /// <summary>
        /// The _hourly timer
        /// </summary>
        private Timer _hourlyTimer;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUpdater"/> class.
        /// </summary>
        public AppUpdater()
        {
            VersionList = new List<VersionModel>();
            _webClient = new WebClient();
            IsUpdateAvailable = false;

            // Initialize needed tasks
            Initialize();
            
            _hourlyTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds
            _hourlyTimer.Elapsed += HourlyTimerOnElapsed;
            _hourlyTimer.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AppUpdater"/> class.
        /// </summary>
        ~AppUpdater()
        {
            _hourlyTimer.Stop();
            _hourlyTimer.Dispose();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            // Get current version
            var thisApp = Assembly.GetExecutingAssembly();
            AssemblyName name = new AssemblyName(thisApp.FullName);
            _currentVersion = name.Version;

            _webClient.Headers.Add("Client-Version", _currentVersion.ToString());
        }

        #endregion

        /// <summary>
        /// Checks version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private async void HourlyTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Kind of a redundant and hacky solution
            // since it might check the time when it's known before hand it won't need to check the versions
            // but it works, and the resources needed are dismissable

            var period = Properties.Settings.Default.AutomaticCheckPeriod;
            var lastCheck = Properties.Settings.Default.LastVersionCheck;
            
            if (period == 1 && lastCheck.AddDays(1) < DateTime.Now)
            {
                await CheckVersions();
            }
            else if (period == 2 && lastCheck.AddDays(7) < DateTime.Now)
            {
                await CheckVersions();
            }
        }

        /// <summary>
        /// Checks the version
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task CheckVersions(Type targetSubscriber = null)
        {
            // If 3 mins passed, update info
            if (targetSubscriber == typeof(UpdateMenuViewModel) && Properties.Settings.Default.LastVersionCheck.AddSeconds(30) <= DateTime.Now
                || Properties.Settings.Default.LastVersionCheck.AddMinutes(3) <= DateTime.Now)
            {
                // Initialize an HttpWebRequest for the current URL.
                var webReq = (HttpWebRequest) WebRequest.Create(ConstantsBase.ApiUrl + "Version");
                webReq.Timeout = 1000;
                webReq.Headers.Add("Client-Version", _currentVersion.ToString());

                // Send request
                using (WebResponse response = await webReq.GetResponseAsync())
                {
                    // Get response
                    var respStream = response.GetResponseStream();

                    // The downloaded resource ends up in the variable named content.
                    var content = new MemoryStream();

                    // Read response if stream is not null
                    if (respStream != null)
                    {
                        await respStream.CopyToAsync(content);
                    }

                    // Deserialize string
                    string str = Encoding.Default.GetString(content.ToArray());
                    VersionList.Clear();
                    VersionList = JsonConvert.DeserializeObject<List<VersionModel>>(str);

                    // Update AppUpdater hash
                    this._appUpdaterHash = response.Headers["App-Checksum"];

                    content.Dispose();
                }

                // Update last check time
                Properties.Settings.Default.LastVersionCheck = DateTime.Now;
                Properties.Settings.Default.Save();

            }

            if (VersionList == null || !VersionList.Any())
                return;

            // Get latest version
            LatestVersion = VersionList.MaxBy(x => x.VersionNumber);

            if (CheckVersionsCompleted != null)
            {
                bool update = LatestVersion.VersionNumber > _currentVersion;
                IsUpdateAvailable = update;
                CheckVersionsCompleted(this, new CheckVersionEventArgs(update, targetSubscriber));
            }
        }

        /// <summary>
        /// Downloads the version package.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task DownloadLatestVersion()
        {
            if (!VersionList.Any())
            {
                await CheckVersions();
            }

            // Get latest version
            LatestVersion = VersionList.MaxBy(x => x.VersionNumber);

            if (LatestVersion == null)
                throw new InvalidOperationException(
                    "A version wasn't found after successfully reaching the server, this should not happen!");

            // Check if package already is downloaded
            if (File.Exists(ConstantsBase.UpdatePath) &&
                String.Equals(CrcHelper.GetCrc32HashToString(ConstantsBase.UpdatePath), LatestVersion.CrcHash,
                    StringComparison.CurrentCultureIgnoreCase))
            {
                // Used to raise DownloadCompleted
                _webClient.CancelAsync();
                return;
            }

            // Iterate URLs
            foreach (var url in LatestVersion.DownloadUrls)
            {
                Directory.CreateDirectory("tmp");

                // Delete file if existent
                if (File.Exists(ConstantsBase.UpdatePath))
                    File.Delete(ConstantsBase.UpdatePath);

                // Download file
                await _webClient.DownloadFileTaskAsync(new Uri(url), ConstantsBase.UpdatePath);

                if (CrcHelper.GetCrc32HashToString(ConstantsBase.UpdatePath) != LatestVersion.CrcHash)
                    continue;
            }

            if (!String.Equals(CrcHelper.GetCrc32HashToString(ConstantsBase.UpdatePath), LatestVersion.CrcHash, StringComparison.CurrentCultureIgnoreCase))
                throw new IOException("Could not successfully download the update package! Data corruption occurred! ");
        }

        /// <summary>
        /// Installs the latest version.
        /// </summary>
        public async void InstallLatestVersion()
        {
            // Check if appupdater is up to date
            if (!File.Exists("AppUpdater.exe") || !String.Equals(CrcHelper.GetCrc32HashToString("AppUpdater.exe"), _appUpdaterHash,
                StringComparison.CurrentCultureIgnoreCase))
            {
                await _webClient.DownloadFileTaskAsync(new Uri(ConstantsBase.ApiUrl + "File/APP"), "appupdater.exe");
            }

            Process.Start("appupdater.exe", Process.GetCurrentProcess().Id.ToString());
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// Finished version list retrieval
        /// </summary>
        public event CheckVersionHandler CheckVersionsCompleted;

        #region Download events

        /// <summary>
        /// Occurs when [download progress changed].
        /// </summary>
        public event DownloadProgressChangedEventHandler DownloadProgressChanged
        {
            add { _webClient.DownloadProgressChanged += value; }
            remove { _webClient.DownloadProgressChanged -= value; }
        }

        /// <summary>
        /// Occurs when [download completed].AsyncCompletedEventHandler
        /// </summary>
        public event AsyncCompletedEventHandler DownloadCompleted
        {
            add { _webClient.DownloadFileCompleted += value; }
            remove { _webClient.DownloadFileCompleted -= value; }
        }

        #endregion

        /// <summary>
        /// Determines whether [is update package available].
        /// </summary>
        /// <returns></returns>
        public bool IsUpdatePackageAvailable()
        {
            return LatestVersion != null && File.Exists(ConstantsBase.UpdatePath) &&
                   String.Equals(CrcHelper.GetCrc32HashToString(ConstantsBase.UpdatePath), LatestVersion.CrcHash,
                       StringComparison.CurrentCultureIgnoreCase);
        }

        #region Properties

        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        /// <value>
        /// The versions.
        /// </value>
        public List<VersionModel> VersionList { get; set; }

        /// <summary>
        /// Gets or sets the latest version.
        /// </summary>
        /// <value>
        /// The latest version.
        /// </value>
        public VersionModel LatestVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is update available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is update available; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdateAvailable { get; set; }

        #endregion

        #region ToString override

        /// <summary>
        /// Returns a string representation of this
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string build = "";

            foreach (var version in VersionList)
            {
                build += String.Format("Version {0} released on {1}:\n{2}\n\n", version.VersionNumber,
                    version.ReleaseDate.ToString("dd-MM-yyyy"), version.Changelog);
            }

            return build;
        }

        #endregion
    }
}