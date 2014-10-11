using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
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
