using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EterManager.Utilities
{
    class CrcHelper
    {
        /// <summary>
        /// Calculates hash from file on disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetCrc32HashToString(string file)
        {
            var crc32 = new Crc32();
            var hash = String.Empty;

            using (var fs = File.Open(file, FileMode.Open))
                hash = crc32.ComputeHash(fs).Aggregate(hash, (current, b) => current + b.ToString("X2").ToLower());

            return hash;
        }

        /// <summary>
        /// Calculates hash from filepath
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetCrc32HashToByteArray(string file)
        {
            var crc32 = new Crc32();
            var hash = String.Empty;

            using (var fs = File.Open(file, FileMode.Open))
                return crc32.ComputeHash(fs);
        }

        /// <summary>
        /// Calculates file from memory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string GetCrc32HashFromMemoryToString(byte[] src)
        {
            var crc32 = new Crc32();

            return crc32.ComputeHash(src).Aggregate(String.Empty, (current, b) => current + b.ToString("X2").ToLower());
        }

        /// <summary>
        /// Calculates hash from memory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] GetCrc32HashFromMemoryToByteArray(byte[] src)
        {
            var crc32 = new Crc32();

            return crc32.ComputeHash(src);
        }
    }

    /// <summary>
    /// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
    /// </summary>
    /// <remarks>
    /// Crc32 should only be used for backward compatibility with older file formats
    /// and algorithms. It is not secure enough for new applications.
    /// If you need to call multiple times for the same data either use the HashAlgorithm
    /// interface or remember that the result of one Compute call needs to be ~ (XOR) before
    /// being passed in as the seed for the next Compute call.
    /// </remarks>
    public sealed class Crc32 : HashAlgorithm
    {
        public const UInt32 DefaultPolynomial = 0xedb88320u;
        public const UInt32 DefaultSeed = 0xffffffffu;

        private static UInt32[] defaultTable;

        private readonly UInt32 seed;
        private readonly UInt32[] table;
        private UInt32 hash;

        public Crc32()
            : this(DefaultPolynomial, DefaultSeed)
        {
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = hash = seed;
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return Compute(DefaultSeed, buffer);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new UInt32[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (UInt32) i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
        {
            var crc = seed;
            for (var i = start; i < size - start; i++)
                crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
            return crc;
        }

        private static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }
}
