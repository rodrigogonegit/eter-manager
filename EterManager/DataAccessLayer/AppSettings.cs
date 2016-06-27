using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EterManager.Base;
using Newtonsoft.Json;

namespace EterManager.DataAccessLayer
{
    public static class AppSettings
    {
        /// <summary>
        /// Initializes the <see cref="AppSettings"/> class.
        /// </summary>
        static AppSettings()
        {
            if (!File.Exists(ConstantsBase.SettingsFilePath))
            {
                File.WriteAllText(ConstantsBase.SettingsFilePath, "");
                return;
            }


        }

        #region Properties      
              
        /// <summary>
        /// Gets or sets the default profile.
        /// </summary>
        /// <value>
        /// The default profile.
        /// </value>
        public static string DefaultProfile { get; set; }

        /// <summary>
        /// Gets or sets the maximum sim files.
        /// </summary>
        /// <value>
        /// The maximum sim files.
        /// </value>
        public static int MaxSimFiles { get; set; }

        /// <summary>
        /// Gets or sets the update mode.
        /// </summary>
        /// <value>
        /// The update mode.
        /// </value>
        public static int UpdateMode { get; set; }

        /// <summary>
        /// Gets or sets the last version check.
        /// </summary>
        /// <value>
        /// The last version check.
        /// </value>
        public static DateTime LastVersionCheck { get; set; }

        /// <summary>
        /// Gets or sets the automatic check period.
        /// </summary>
        /// <value>
        /// The automatic check period.
        /// </value>
        public static int AutomaticCheckPeriod { get; set; }

        #endregion
    }
}
