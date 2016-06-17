using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EterManager.Services.Concrete
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="EterManager.Services.Abstract.IAppUpdater" />
    class AppUpdater : IAppUpdater
    {
        /// <summary>
        /// The _web client
        /// </summary>
        private readonly WebClient _webClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUpdater"/> class.
        /// </summary>
        public AppUpdater()
        {
            VersionList = new List<VersionModel>();
            _webClient = new WebClient();
            _webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
        }

        /// <summary>
        /// Checks the version
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task CheckVersions()
        {
            // If 3 mins passed, update info
            if (Properties.Settings.Default.LastVersionCheck.AddMinutes(3) <= DateTime.Now || !VersionList.Any())
            {
                // Initialize an HttpWebRequest for the current URL.
                var webReq = (HttpWebRequest)WebRequest.Create("http://139.59.148.154:8080/");
                webReq.Timeout = 1000;

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
                    VersionList = JsonConvert.DeserializeObject<List<VersionModel>>(str);

                    content.Dispose();
                }
            }

            // Update last check time
            Properties.Settings.Default.LastVersionCheck = DateTime.Now;

            // Get latest version
            var latestVersion = VersionList.Max(x => x.VersionNumber);

            // Get current version
            var thisApp = Assembly.GetExecutingAssembly();
            AssemblyName name = new AssemblyName(thisApp.FullName);

            if (latestVersion > name.Version)
            {
                if (NewVersionFound != null)
                    NewVersionFound(this, EventArgs.Empty);
            }

    }

        /// <summary>
        /// Downloads the version package.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void DownloadLatestVersion()
        {
            // Get latest version
            VersionModel latestVersion = VersionList.MaxBy(x => x.VersionNumber);

            // Check if package already is downloaded
            if (File.Exists(ConstantsBase.UpdatePath) && String.Equals(CrcHelper.GetCrc32HashToString(ConstantsBase.UpdatePath), latestVersion.CrcHash, StringComparison.CurrentCultureIgnoreCase))
            {
                // Used to raise DownloadCompleted
                _webClient.CancelAsync();
            }

            // Iterate URLs
            foreach (var url in latestVersion.DownloadUrls)
            {
                try
                {
                    Directory.CreateDirectory("/tmp/");

                    // Delete file if existent
                    if (File.Exists(ConstantsBase.UpdatePath))
                        File.Delete(ConstantsBase.UpdatePath);

                    // Download file
                    _webClient.DownloadFileAsync(new Uri(url), ConstantsBase.UpdatePath);
                }
                finally
                {
                    
                }
            }
        }

        /// <summary>
        /// Installs the latest version.
        /// </summary>
        public void InstallLatestVersion()
        {
            Process.Start("appupdater.exe", Process.GetCurrentProcess().Id.ToString());
        }

        /// <summary>
        /// Found new version
        /// </summary>
        public event EventHandler NewVersionFound;


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

        #region Properties

        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        /// <value>
        /// The versions.
        /// </value>
        public List<VersionModel> VersionList { get; set; }

        public override string ToString()
        {
            string build = "";

            foreach (var version in VersionList)
            {
                build += String.Format("Version {0} released on {1}:\n{2}\n\n", version.VersionNumber, version.ReleaseDate.ToString("dd-MM-yyyy"), version.Changelog);
            }
           
            return build;
        }

        #endregion
    }

}
