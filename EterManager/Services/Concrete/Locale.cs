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
            _localeTokens.Add("ERROR_SAVING_PROFILE", "An error occured while saving {0} profile.");
            _localeTokens.Add("INTERNAL_ERROR", "INTERNAL ERROR");

        }

        #endregion
    }
}
