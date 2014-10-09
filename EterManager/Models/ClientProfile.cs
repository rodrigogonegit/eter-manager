using System;
using System.IO;
using System.Xml.Serialization;
using EterManager.Base;
using Ninject.Activation.Providers;

namespace EterManager.Models
{
    public class ClientProfile
    {
        [XmlIgnore]
        public string OriginalName { get; set; }

        private string _name;
        [XmlElement(IsNullable = true)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (OriginalName == null)
                    OriginalName = value;
                _name = value;
            }
        }

        [XmlElement(IsNullable = true)]
        public string WorkingDirectory { get; set; }

        [XmlElement(IsNullable = true)]
        public string UnpackDirectory { get; set; }

        [XmlElement(IsNullable = true)]
        public byte[] IndexKey { get; set; }

        [XmlElement(IsNullable = true)]
        public byte[] PackKey { get; set; }

        [XmlElement(IsNullable = true)]
        public string IndexExtension { get; set; }

        [XmlElement(IsNullable = true)]
        public string PackExtension { get; set; }

        #region Methods

        public void Save()
        {
            // Rename file if necessary
            if (OriginalName != Name)
            {
                File.Move(
                    String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, OriginalName), 
                    String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name));

                OriginalName = Name;
            }

            // Create file stream
            using (var writer = new StreamWriter(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name)))
            {
                var serializer = new XmlSerializer(typeof(ClientProfile));
                serializer.Serialize(writer, this);
            }

        }
        #endregion

        #region Constructors

        #endregion
    }
}
