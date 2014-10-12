using System;

namespace EterManager.Utilities
{
    class Xtea
    {
        /// <summary>
        /// Decrypts data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="encryptedData"></param>
        /// <param name="plainData"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public static Boolean Decrypt(byte[] key, byte[] encryptedData, byte[] plainData, uint dataSize)
        {
            if (dataSize == 0) return false;

            dataSize &= 0xFFFFFFF8;
            dataSize /= 8;
            UInt32 i;
            var v = new UInt32[2];
            var lkey = new UInt32[4];
            int eoff = 0;
            int poff = 0;

            Buffer.BlockCopy(key, 0, lkey, 0, 16);
            const UInt32 delta = 0x9E3779B9;
            while (dataSize-- != 0)
            {
                Buffer.BlockCopy(encryptedData, eoff, v, 0, 8);
                UInt32 sum = 0xC6EF3720;

                for (i = 0; i < 32; i++)
                {
                    v[1] -= (v[0] << 4 ^ v[0] >> 5) + v[0] ^ sum + lkey[sum >> 11 & 3];
                    sum -= delta;
                    v[0] -= (v[1] << 4 ^ v[1] >> 5) + v[1] ^ sum + lkey[sum & 3];
                }

                Buffer.BlockCopy(v, 0, plainData, poff, 8);
                poff += 8;
                eoff += 8;
            }

            return true;
        }
        /// <summary>
        /// Encrypts data (second implementation, using a referenced byte[])
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public static void Encrypt2(ref byte[] data, byte[] key)
        {
            if (key.Length <= 15)
                return;

            var k = new uint[4] { (uint)((key[3] << 24) + (key[2] << 16) + (key[1] << 8) + (key[0] << 0)),
                    (uint)((key[7] << 24) + (key[6] << 16) + (key[5] << 8) + (key[4] << 0)),
                    (uint)((key[11] << 24) + (key[10] << 16) + (key[9] << 8) + (key[8] << 0)),
                    (uint)((key[15] << 24) + (key[14] << 16) + (key[13] << 8) + (key[12] << 0))};

            for (int i = 0; i < data.Length; i += 8)
            {
                var v0 = (uint)(data[i] + (data[i + 1] << 8) + (data[i + 2] << 16) + (data[i + 3] << 24));
                var v1 = (uint)(data[i + 4] + (data[i + 5] << 8) + (data[i + 6] << 16) + (data[i + 7] << 24));
                const uint delta = 0x9E3779B9;
                uint sum = 0;

                for (int r = 0; r < 32; r++)
                {
                    v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + k[sum & 3]);
                    sum += delta;
                    v1 += (v0 << 4 ^ v0 >> 5) + v0 ^ sum + k[sum >> 11 & 3];
                }

                data[i] = (byte)v0;
                data[i + 1] = (byte)(v0 >> 8);
                data[i + 2] = (byte)(v0 >> 16);
                data[i + 3] = (byte)(v0 >> 24);
                data[i + 4] = (byte)v1;
                data[i + 5] = (byte)(v1 >> 8);
                data[i + 6] = (byte)(v1 >> 16);
                data[i + 7] = (byte)(v1 >> 24);
            }
        }
    }
}
