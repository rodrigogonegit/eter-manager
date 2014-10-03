using System.Xml.Serialization;

namespace EterManager.Models
{
    public class ClientProfile
    {
        [XmlElement(IsNullable = true)]
        public string Name { get; set; }

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

        #endregion
    }
}
