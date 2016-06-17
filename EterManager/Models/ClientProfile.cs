using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using EterManager.Base;
using Ninject.Activation.Providers;

namespace EterManager.Models
{

    public class ClientProfile
    {
        #region Properties

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

        [DefaultValue(false)]
        public bool IsDefault { get; set; }

        #endregion

        #region Constructors

        #endregion

        #region Methods

        /// <summary>
        /// Saves profile
        /// </summary>
        public void Save()
        {            
            if (!IsUniqueName())
                throw new ProfileNameAlreadyExistsException(Name);

            // Rename file if necessary
            if (OriginalName != Name)
            {
                if (File.Exists(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, OriginalName)))
                {
                    File.Move(
                        String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, OriginalName),
                        String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name));

                }

                OriginalName = Name;
            }

            // Create file stream
            using (var writer = new StreamWriter(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name)))
            {
                var serializer = new XmlSerializer(typeof(ClientProfile));
                serializer.Serialize(writer, this);
            }
        }

        /// <summary>
        /// Deletes the associated file
        /// </summary>
        public void Remove()
        {
            File.Delete(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, Name));
        }

        /// <summary>
        /// Checks if name is in use
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsUniqueName(string name = null)
        {
            // Check if profile with the same name exists
            return !File.Exists(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, name ?? Name)) || OriginalName == Name;
        }

        /// <summary>
        /// Gets all profiles
        /// </summary>
        /// <returns></returns>
        public static IList<ClientProfile> GetAllProfiles()
        {
            var list = new List<ClientProfile>();

            // Create serializer
            var deserializer = new XmlSerializer(typeof(ClientProfile));

            Directory.CreateDirectory(ConstantsBase.ProfilesPath);

            // Loop through directory's files
            foreach (var file in new DirectoryInfo(ConstantsBase.ProfilesPath).GetFiles("*.xml"))
            {
                using (var stream = new StreamReader(file.FullName))
                {
                    list.Add(deserializer.Deserialize(stream) as ClientProfile);
                }
            }
        
            return list;
        }

        /// <summary>
        /// Gets profiles matching the specified criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static ClientProfile GetProfileByPredicate(Func<ClientProfile, bool> p)
        {
            return GetAllProfiles().FirstOrDefault(p);
        }

        /// <summary>
        /// Creates default profile
        /// </summary>
        public static ClientProfile CreateDefaultProfile()
        {
            var p = new ClientProfile()
            {
                Name = "Default Profile",
                IndexKey = new byte[] { 0xB9, 0x9E, 0xB0, 0x02, 0x6F, 0x69, 0x81, 0x05, 0x63, 0x98, 0x9B, 0x28, 0x79, 0x18, 0x1A, 0x00 },
                PackKey = new byte[] { 0x22, 0xB8, 0xB4, 0x04, 0x64, 0xB2, 0x6E, 0x1F, 0xAE, 0xEA, 0x18, 0x00, 0xA6, 0xF6, 0xFB, 0x1C },
                PackExtension = ".epk",
                IndexExtension = ".eix",
                WorkingDirectory = "WORKING_DIR",
                UnpackDirectory = "UNPACK_DIR"
            };

            var counter = 1;

            while (!p.IsUniqueName())
            {
                p.Name = p.Name + counter++;
            }

            // Create file stream
            using (var writer = new StreamWriter(String.Format("{0}{1}.xml", ConstantsBase.ProfilesPath, p.Name)))
            {
                var serializer = new XmlSerializer(typeof(ClientProfile));
                serializer.Serialize(writer, p);
            }

            return p;
        }

        #endregion

        
    }
}
