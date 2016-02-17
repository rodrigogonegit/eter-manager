using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Services
{

    class Locale : ILocale
    {
        #region Fields

        private readonly Dictionary<string, string> _localeTokens = new Dictionary<string, string>();

        #endregion

        public string GetString(string identifier)
        {
            // If null, gets initialized by private membere
            if (_localeTokens.Count == 0)
                Initialize();

            // Try to get value
            string rtnValue;

            _localeTokens.TryGetValue(identifier, out rtnValue);

            // Return value
            return rtnValue;
        }

        #region Private Methods

        private void Initialize()
        {
            //_localeTokens.Add("", "");
            _localeTokens.Add("DROP_EIX_OR_DIR", "Only eter index (eix) files or directories are supported!");
            _localeTokens.Add("SELECT_PROFILE_FIRST", "Select a profile first!");
            _localeTokens.Add("ERROR_WITH_CUSTOM_MSG", "{0}");
            _localeTokens.Add("COULD_NOT_MONITOR_DIR", "Could not instantiate a monitoring object on the specified path: {0}");
            _localeTokens.Add("FILE_NOT_FOUND", "The specified file could not be reached: {0}");
            _localeTokens.Add("ETER_EPK_FILE_NOT_FOUND", "Eter pack file not found!");
            _localeTokens.Add("ETER_WRONG_INDEX_KEY", "Wrong index key!");
            _localeTokens.Add("INVALID_PROFILE_SETTINGS", "Invalid profile settings!");
            _localeTokens.Add("DIR_NOT_FOUND", "The specified directory could not be reached: {0}");
            _localeTokens.Add("FILE_TOO_BIG", "File is too big.");
            _localeTokens.Add("ERROR_READING_PATH", "Error reading path: {0}");
            _localeTokens.Add("FILE_ALREADY_PACKED_BUT_OVER", "File already exists and will be overwritten!");
            _localeTokens.Add("USER_SELECT_PACK_TYPE", "Select a pack type!");
            _localeTokens.Add("ETER_UNPACK_RESULT", "Files unpacked successfully: {0} / Hash mismatch: {1} / Failed to process: {2}");
            _localeTokens.Add("ETER_PACK_RESULT", "Files packed successfully: {0} / Hash mismatch: {1} / Failed to process: {2}");
            _localeTokens.Add("NEW_DRIVE_POINT_ADDED", "New drive point found and saved: {0}!");
            _localeTokens.Add("PROFILE_SAVED", "Profile {0} saved!");
            _localeTokens.Add("COULD_NOT_ACCESS_FILE", "Could not access file. {0}");
            _localeTokens.Add("ERROR_SAVING_PROFILE", "An error occured while saving {0} profile.");
            _localeTokens.Add("INTERNAL_ERROR", "INTERNAL ERROR");
            _localeTokens.Add("PROFILE_NAME_ALREADY_EXISTS", "Profile name already exists, please choose a different one.");
        }

        #endregion
    }
}
