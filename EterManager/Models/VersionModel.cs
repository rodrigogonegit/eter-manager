using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EterManager.Models
{
    [Serializable]
    public class VersionModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        /// <value>
        /// The version number.
        /// </value>
        [JsonProperty("version_number")]
        public Version VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the changelog.
        /// </summary>
        /// <value>
        /// The changelog.
        /// </value>
        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        /// <summary>
        /// Gets or sets the download urls.
        /// </summary>
        /// <value>
        /// The download urls.
        /// </value>
        [JsonProperty("download_url")]
        public List<string> DownloadUrls { get; set; }

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        /// <value>
        /// The release date.
        [JsonProperty("release_date")]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the CRC hash.
        /// </summary>
        /// <value>
        /// The CRC hash.
        /// </value>
        [JsonProperty("package_crc_hash")]
        public string CrcHash { get; set; }

        #endregion
    }
}
