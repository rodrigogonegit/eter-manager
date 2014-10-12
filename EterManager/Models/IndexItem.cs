using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Models
{
    class IndexItem
    {
        public IndexItem(int index, string fileName, string fileNameCRC, int diskSize, int size, string crc, int fileOffset, int packType, string parentFile)
        {
            this.Index = index;
            this.Filename = fileName;
            this.FilenameCRC = fileNameCRC;
            this.DiskSize = diskSize;
            this.Size = size;
            this.CRCHash = crc;
            this.Offset = fileOffset;
            this.PackType = packType;
            this.ParentFile = parentFile;
        }

        #region "Properties"

        public int Index { get; set; }

        public string Filename { get; set; }

        public string FilenameCRC { get; set; }
        public int DiskSize { get; set; }

        public int Size { get; set; }

        public string CRCHash { get; set; }

        public int Offset { get; set; }

        public int PackType { get; set; }

        public string ParentFile { get; set; }

        #endregion

    }
}
