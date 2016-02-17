using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Exception thrown when a profile with the same name already exists
    /// </summary>
    class ProfileNameAlreadyExistsException : Exception
    {
        public string ProfileName { get; set; }
        public ProfileNameAlreadyExistsException(string profileName)
        {
            ProfileName = profileName;
        }

        public override string ToString()
        {
            return String.Format("Profile name \"{0}\" already exists!", ProfileName);
        }
    }
}
