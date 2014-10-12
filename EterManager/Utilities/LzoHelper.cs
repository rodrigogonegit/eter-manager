using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace EterManager.Utilities
{
    class LzoHelper
    {
        [DllImport(@"LzoModule.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Decompress(IntPtr source, int compressedSize, int decompressedSize, IntPtr decompressedFilePointer);

        [DllImport(@"LzoModule.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Compress(IntPtr src, int srcLen, ref int realCompressionSize, IntPtr decompressedFilePointer);


        /// <summary>
        /// GLZO wrapper to decompress data
        /// </summary>
        /// <param name="decompressedSize"></param>
        /// <param name="compressedSize"></param>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public static byte[] DecompressData(int decompressedSize, int compressedSize, byte[] compressedData)
        {
            //Pointer to hold the compressed data (source)
            IntPtr compressedDataPointer = Marshal.AllocHGlobal(compressedData.Length);

            IntPtr decompressedFilePointer = Marshal.AllocHGlobal(decompressedSize);

            //Converting from byte[] to a pointer
            Marshal.Copy(compressedData, 0, compressedDataPointer, compressedData.Length);

            //Assign the decompressedData to the decompressed data pointer
            Decompress(compressedDataPointer, compressedSize, decompressedSize, decompressedFilePointer);

            //Creating a byte[] buffer to hold the decompressed data
            byte[] decompressedFileBuffer = new byte[decompressedSize];

            //Copy content from decompressedData pointer to decomrpessed byte[] buffer
            Marshal.Copy(decompressedFilePointer, decompressedFileBuffer, 0, decompressedSize);

            //Free memory
            Marshal.FreeHGlobal(compressedDataPointer);
            Marshal.FreeHGlobal(decompressedFilePointer);

            //Return decompressedData
            return decompressedFileBuffer;
        }

        /// <summary>
        /// Wrapper function to compress data
        /// </summary>
        /// <param name="srcLenght"></param>
        /// <param name="decompressedData"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static byte[] CompressData(int srcLenght, byte[] decompressedData)
        {
            // Pointer to hold decompressed data
            IntPtr decompressedDataPointer = Marshal.AllocHGlobal(srcLenght);

            // Pointer to hold compressed data
            IntPtr compressedDataPointer = Marshal.AllocHGlobal(srcLenght + (srcLenght / 16) + 64 + 3);

            // Convert from byte[] to pointer
            Marshal.Copy(decompressedData, 0, decompressedDataPointer, srcLenght);

            // Value passed as reference to hold actual compressed data's size
            int realSize = 0;

            // Unmanaged code compressing data and assgning it to compressedDataPointer
            Compress(decompressedDataPointer, srcLenght, ref realSize, compressedDataPointer);

            // Create byte[] to hold compressed data
            byte[] compressedDataBuffer = new byte[srcLenght + (srcLenght / 16) + 64 + 3];

            // Marshal from pointer to byte[]
            Marshal.Copy(compressedDataPointer, compressedDataBuffer, 0, srcLenght + (srcLenght / 16) + 64 + 3);

            // Free memory
            Marshal.FreeHGlobal(decompressedDataPointer);
            Marshal.FreeHGlobal(compressedDataPointer);

            // Resize to actual compression Size
            Array.Resize(ref compressedDataBuffer, realSize);

            return compressedDataBuffer;
        }
    }
}
