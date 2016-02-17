using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using EterManager.Base;
using EterManager.Exceptions.EterFiles;
using EterManager.Models;
using EterManager.Services.Abstract;
using EterManager.Services.Concrete;
using EterManager.Utilities;

namespace EterManager.DataAccessLayer
{
    class EterFilesDal
    {
        private static readonly IDrivePointManager DrivePointManager =
            ((App) Application.Current).GetInstance<IDrivePointManager>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packFile"></param>
        /// <param name="saveFilesPath"></param>
        /// <param name="indexKey"></param>
        /// <param name="packKey"></param>
        /// <param name="progressCallback"></param>
        /// <param name="fileLoggingCallback"></param>
        public static void UnpackFile(
            FileInfo packFile,
            string saveFilesPath,
            byte[] indexKey,
            byte[] packKey,
            Action<int, int> progressCallback,
            Action<ErrorItem, string> fileLoggingCallback)
        {
            if (!File.Exists(packFile.FullName))
            {
                throw new EterPackFileNotFoundException();
            }

            // Reads the index file
            List<IndexItem> workingList = ReadIndexFile(EterHelper.ReplaceWithEixExt(packFile.FullName), indexKey, StringHelpers.TrimExtension(packFile.Name));

            if (workingList == null)
                return;

            // Check userData paths
            Directory.CreateDirectory(saveFilesPath);

            // File result counters
            double totalFilesCounter = 0;
            int unnamedFilesCounter = 0;
            int operationResult = 0;

            foreach (IndexItem item in workingList)
            {
                if (item.Size < 16 && item.PackType > 0)
                {
                    //Logger.LogOutputMessage(Log.LogId.FILE_SIZE_TOO_SMALL, args: new object[] { item.Filename, item.Size }); TODO
                    continue;
                }

                DrivePointManager.CheckIfContainsDrivePoint(item.Filename);

                if (String.IsNullOrWhiteSpace(item.Filename))
                {
                    unnamedFilesCounter++;
                    continue;
                }

                switch (item.PackType)
                {
                    // Raw format
                    case 0: 
                        operationResult = ProcessFileType0(
                            packFile.FullName,
                            String.Format("{0}{1}/", saveFilesPath, StringHelpers.TrimExtension(packFile.Name)),
                            item,
                            fileLoggingCallback);
                        break;
                    // LZO compressed file
                    case 1:
                        operationResult = ProcessFileType1(
                            packFile.FullName,
                            String.Format("{0}{1}/", saveFilesPath, StringHelpers.TrimExtension(packFile.Name)),
                            item,
                            fileLoggingCallback);
                        break;
                    // XTEA encrypted and LZO compressed file
                    case 2: 
                        operationResult = ProcessFileType2(
                            packFile.FullName,
                            String.Format("{0}{1}/", saveFilesPath, StringHelpers.TrimExtension(packFile.Name)),
                            item,
                            packKey,
                            fileLoggingCallback);
                        break;
                    default:
                        fileLoggingCallback(
                            new ErrorItem(item.Filename, String.Format("Type {0} is not yet supported.", item.PackType)),
                            null);
                        break;
                }

                totalFilesCounter += 1.0;

                // Update progress
                double actionProgress = (totalFilesCounter / workingList.Count * 100.0);
                progressCallback(operationResult, (int)actionProgress);
            }

            // If unnamed files were found, log it
            if (unnamedFilesCounter > 0)
                fileLoggingCallback(
                    new ErrorItem("UNDEFINED", unnamedFilesCounter + " files were ignored due to incomplete header info (no name)", unnamedFilesCounter),
                    null);
        }

        #region File Processing Stuff (types supported: 0 to 2)

        /// <summary>
        /// Processes files packed as type 0 (packed)
        /// </summary>
        /// <param name="packFilePath">Path to EPK file (name included)</param>
        /// <param name="saveFilesPath">Path to where the file should be saved (name not included)</param>
        /// <param name="item">Item from deserialized list</param>
        /// <param name="hashMismatchFiles">Vector holding files with CRC mismatch</param>
        /// <param name="errorFiles">Vector holding files ignored</param>
        /// <returns></returns>
        public static int ProcessFileType0(string packFilePath, string saveFilesPath, IndexItem item, Action<ErrorItem, string> fileLoggingCallback)
        {
            int valueToReturn = 0;
            byte[] rawFileBuffer = IOHelper.ReadFileOffsetToLength(packFilePath, item.Offset, item.Size);

            string path = DrivePointManager.RemoveDrivePoints((saveFilesPath + item.Filename).Replace("\\", "/"));

            string foldersPath = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));

            //Does the target directory exist?
            Directory.CreateDirectory(foldersPath);

            // Overwrite file TODO SAME AS TYPE 2
            if (File.Exists(path))
            {
                if (IOHelper.IsFileLocked(path))
                {
                    //Logger.LogOutputMessage(Log.LogId.FILE_OPENED_BY_EXTERNAL_PROCESS_LOG, args: path); TODO
                    return 2;
                }
                File.Delete(path);
            }

            //Check hashes
            if (!CompareCrcHashes(rawFileBuffer, item.CRCHash))
            {
                //Log CRC mismatch
                fileLoggingCallback(null, item.Filename);
                valueToReturn = 1;
            }

            //Write file to disk
            File.WriteAllBytes(path, rawFileBuffer);
            return valueToReturn;

        }

        /// <summary>
        /// Processes files packed as type 1 (LZO compressed)
        /// </summary>
        /// <param name="packFilePath">Path to EPK file (name included)</param>
        /// <param name="saveFilesPath">Path to where the file should be saved (name not included)</param>
        /// <param name="item">Item from deserialized list</param>
        /// <param name="hashMismatchFiles">Vector holding files with CRC mismatch</param>
        /// <param name="errorFiles">Vector holding files ignored</param>
        /// <returns></returns>
        public static int ProcessFileType1(string packFilePath, string saveFilesPath, IndexItem item, Action<ErrorItem, string> fileLoggingCallback)
        {
            int valueToReturn = 0;
            byte[] fileHeaderBuffer = IOHelper.ReadFileOffsetToLength(packFilePath, item.Offset, 16);

            // Read FourCC value
            byte[] fourCC = new byte[4];
            Array.Copy(fileHeaderBuffer, 0, fourCC, 0, 4);

            // Get sizes from header
            int encryptedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 4, 4);
            int compressedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 8, 4);
            int decompressedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 12, 4);

            // Compare FourCC header
            if (!fourCC.SequenceEqual(ConstantsBase.LzoFourCc))
            {
                fileLoggingCallback(new ErrorItem(item.Filename, String.Format("Invalid FourCC: {0}. Expected: {1} ({2})", 
                        BitConverter.ToString(fourCC),
                        BitConverter.ToString(ConstantsBase.LzoFourCc),
                        Encoding.ASCII.GetString(ConstantsBase.LzoFourCc))),
                    "");
                return 2;
            }

            if (encryptedSize != 0 || compressedSize <= 0 || decompressedSize <= 0)
            {
                fileLoggingCallback(new ErrorItem(item.Filename, String.Format("Invalid header sizes (enc: {0} / cmpr: {1} / decompr: {2}", encryptedSize, compressedSize, decompressedSize)), "");
                return 2;
            }

            // Sizes capped at 629145600
            #region Cap Check
            if (encryptedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(encryptedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }

            if (compressedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(compressedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }

            if (decompressedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(decompressedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }
            #endregion

            // Take care of relative path and folders
            string path = DrivePointManager.RemoveDrivePoints((saveFilesPath + item.Filename).Replace("\\", "/"));

            string foldersPath = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));

            // Get actual data
            byte[] rawFileBuffer = IOHelper.ReadFileOffsetToLength(packFilePath, item.Offset + 20, compressedSize);

            // Target directory exists?
            Directory.CreateDirectory(foldersPath);

            // Overwrite file TODO SAME AS TYPE 2
            if (File.Exists(path))
            {
                if (IOHelper.IsFileLocked(path))
                {
                    //Logger.LogOutputMessage(Log.LogId.FILE_OPENED_BY_EXTERNAL_PROCESS_LOG, args: path); TODO
                    return 2;
                }
                File.Delete(path);
            }

            // Decompress data
            byte[] decompressedDataBuffer = LzoHelper.DecompressData(decompressedSize, compressedSize, rawFileBuffer);

            // Check hash
            if (!CompareCrcHashes(decompressedDataBuffer, item.CRCHash))
            {
                // Log CRC mismatch
                fileLoggingCallback(null, item.Filename);
                valueToReturn = 1;
            }

            // Write file to disk
            File.WriteAllBytes(path, decompressedDataBuffer);

            return valueToReturn;
        }

        /// <summary>
        /// Processes files packed as type 2 (encrypted & compressed)
        /// </summary>
        /// <param name="packFilePath">Path to EPK file (name included)</param>
        /// <param name="saveFilesPath">Path to where the file should be saved (name not included)</param>
        /// <param name="item">Item from deserialized list</param>
        /// <param name="packKey">Pack XTEA key</param>
        /// <param name="hashMismatchFiles">Vector holding files with CRC mismatch</param>
        /// <param name="errorFiles">Vector holding files ignored</param>
        /// <returns></returns>
        public static int ProcessFileType2(string packFilePath, string saveFilesPath, IndexItem item, byte[] packKey, Action<ErrorItem, string> fileLoggingCallback)
        {
            int valueToReturn = 0;

            byte[] fileHeaderBuffer = IOHelper.ReadFileOffsetToLength(packFilePath, item.Offset, 16);

            // Read FourCC value
            var fourCc = new byte[4];
            Array.Copy(fileHeaderBuffer, 0, fourCc, 0, 4);

            // Get sizes from header
            int encryptedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 4, 4);
            int compressedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 8, 4);
            int decompressedSize = IOHelper.ReadIntFromArray(fileHeaderBuffer, 12, 4);

            if (encryptedSize <= 0 || compressedSize <= 0 || decompressedSize <= 0)
            {
                fileLoggingCallback(new ErrorItem(item.Filename, "Invalid encrypted/compressed/decompressed size."), null);
                return 2;
            }

            #region Cap Check
            if (encryptedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(encryptedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }

            if (compressedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(encryptedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }

            if (decompressedSize > 629145600)
            {
                fileLoggingCallback(new ErrorItem(item.Filename,
                    String.Format("Max memory allocation reached, tried to allocate: {0} mb",
                        Math.Round(encryptedSize / 1024.0 / 1024.0, 2))), null);
                return 2;
            }
            #endregion

            // Is header correct?
            if (!fourCc.SequenceEqual(ConstantsBase.LzoFourCc))
            {
                // Show error msg and cancel operation
                // AppLog.SendMessage(8, item.Filename, item.ParentFile); TODO
                fileLoggingCallback(new ErrorItem(item.Filename, String.Format("Invalid FourCC: {0}. Expected: {1} ({2})",
                    BitConverter.ToString(fourCc),
                    BitConverter.ToString(ConstantsBase.LzoFourCc),
                    Encoding.ASCII.GetString(ConstantsBase.LzoFourCc))),
                "");
                return 2;
            }

            // Get actual data
            byte[] rawDataBuffer = IOHelper.ReadFileOffsetToLength(packFilePath, item.Offset + 16, encryptedSize);

            // Take care of relative path and folders
            string path = DrivePointManager.RemoveDrivePoints((saveFilesPath + item.Filename).Replace("\\", "/"));

            // uh, I've found a bug. This should be changed... The program now detects new drive points, for example

            string foldersPath = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));

            // Check if paths exist
            Directory.CreateDirectory(foldersPath);

            // Overwrite file TODO SAME AS TYPE 2
            if (File.Exists(path))
            {
                if (IOHelper.IsFileLocked(path))
                {
                    //Logger.LogOutputMessage(Log.LogId.FILE_OPENED_BY_EXTERNAL_PROCESS_LOG, args: path); TODO
                    return 2;
                }
                File.Delete(path);
            }

            // Create encryption and encryption without header buffers
            var decryptedBuffer = new byte[encryptedSize];
            var decryptedBufferWithoutHeader = new byte[encryptedSize - 4];

            // It should never return false
            if (Xtea.Decrypt(packKey, rawDataBuffer, decryptedBuffer, (uint)encryptedSize) == false)
            {
                throw new Exception("INTERNAL ERROR - DECRYPTION FAILED");
            }

            // Get header after decryption
            var headerAfterDecryption = new byte[4];
            Array.Copy(decryptedBuffer, 0, headerAfterDecryption, 0, 4);

            // Was the encryption key correct?
            if (!headerAfterDecryption.SequenceEqual(ConstantsBase.LzoFourCc))
            {
                // Send message and return error
                //Logger.LogOutputMessage(Log.LogId.DECRYPTION_FAILED_LOG, args: item.Filename); TODO
                fileLoggingCallback(new ErrorItem(item.Filename, String.Format("Invalid header after decryption: {0}. Expected: {1} ({2})",
                    BitConverter.ToString(fourCc),
                    BitConverter.ToString(ConstantsBase.LzoFourCc),
                    Encoding.ASCII.GetString(ConstantsBase.LzoFourCc))),
                "");
                valueToReturn = 2;
            }

            // Get actual decrypted data
            Array.Copy(decryptedBuffer, 4, decryptedBufferWithoutHeader, 0, decryptedBuffer.Length - 4);

            // Check hash
            if (!CompareCrcHashes(decryptedBufferWithoutHeader, item.CRCHash))
            {
                // Log CRC mismatch
                // AppLog.SendMessage(2, item.Filename);
                fileLoggingCallback(null, item.Filename);
                valueToReturn = 1;
            }

            byte[] decompressedData = LzoHelper.DecompressData(decompressedSize, compressedSize, decryptedBufferWithoutHeader);

            // Write file
            File.WriteAllBytes(path, decompressedData);

            return valueToReturn;
        }

        #endregion

        /// <summary>
        /// Normalize index data
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="indexKey">Index XTEA key</param>
        /// <returns></returns>
        public static byte[] NormalizeIndexFile(string filePath, byte[] indexKey)
        {
            //File exists?
            if (!File.Exists(filePath))
            {
                //Logger.LogOutputMessage(Log.LogId.FILE_NOT_FOUND, args: filePath); TODO
                throw new FileNotFoundException(filePath);
            }

            //Read file
            byte[] rawFileBuffer = IOHelper.ReadFile(filePath);

            if (rawFileBuffer.Length < 16)
                return null;

            //Read header value
            var headerCCBuffer = new byte[4];

            Array.Copy(rawFileBuffer, 0, headerCCBuffer, 0, 4);
            int encryptedSize = IOHelper.ReadIntFromArray(rawFileBuffer, 4, 4);
            int compressedSize = IOHelper.ReadIntFromArray(rawFileBuffer, 8, 4);
            int decompressedSize = IOHelper.ReadIntFromArray(rawFileBuffer, 12, 4);

            //Preparing buffer to decrypt
            var dataPtr = new byte[rawFileBuffer.Length - 16];
            Array.Copy(rawFileBuffer, 16, dataPtr, 0, rawFileBuffer.Length - 16);

            var decryptedBuffer = new byte[encryptedSize];

            //Decryption
            if (Xtea.Decrypt(indexKey, dataPtr, decryptedBuffer, (uint)encryptedSize))
            {
                if (decryptedBuffer.Length < 8)
                    return null;

                //Decryption failed?
                Array.Copy(decryptedBuffer, 0, headerCCBuffer, 0, 4);
                if (!headerCCBuffer.SequenceEqual(ConstantsBase.LzoFourCc))
                {
                    throw new ErrorReadingIndexException("Wrong index decryption key");
                }

                //Create buffer without header
                byte[] afterDecryptionBuffer = new byte[decryptedBuffer.Length - 4];
                Array.Copy(decryptedBuffer, 4, afterDecryptionBuffer, 0, decryptedBuffer.Length - 4);

                //Decompress data
                byte[] normalizedFileBuffer = LzoHelper.DecompressData(decompressedSize, compressedSize, afterDecryptionBuffer);
                Array.Copy(normalizedFileBuffer, 0, headerCCBuffer, 0, 4);

                //Decompression failed?
                if (!headerCCBuffer.SequenceEqual(ConstantsBase.EterFourCc))
                {
                    throw new ErrorReadingIndexException("An internal error occured when decompressing");
                }

                //Return normalized data
                return normalizedFileBuffer;
            }

            //Should NEVER happen
            return null;
        }

        /// <summary>
        /// Reads index file and creates deserialized data
        /// </summary>
        /// <param name="filePath">Path to index file</param>
        /// <param name="indexKey">Index XTEA key</param>
        /// <param name="parentFile">Parent file</param>
        /// <returns></returns>
        public static List<IndexItem> ReadIndexFile(string filePath, byte[] indexKey, string parentFile)
        {
            byte[] plainData = NormalizeIndexFile(filePath, indexKey);

            if (plainData == null)
                return null;

            var rtnList = new List<IndexItem>();

            rtnList.Clear();

            //Remove header
            var dataBuffer = new byte[plainData.Length - 12];
            Array.Copy(plainData, 12, dataBuffer, 0, plainData.Length - 12);

            //Convert byte[] to int
            int fileCount = IOHelper.ReadIntFromArray(plainData, 8, 4);
            int indexVersion = IOHelper.ReadIntFromArray(plainData, 4, 4);

            if (indexVersion != 2)
            {
                //Logger.LogOutputMessage(Log.LogId.ETER_INDEX_WRONG_IDX_VERSION_LOG, args: fileCount); TODO
                return null;
            }

            //Holding variables
            var Index = new byte[4];
            var FileName = new byte[161];
            var FilenameCRC = new byte[4];
            var DiskSize = new byte[4];
            var DataSize = new byte[4];
            var CRC = new byte[4];
            var FileOffset = new byte[4];
            var PackedType = new byte[1];

            //Loop through files
            for (int i = 0; i < fileCount; i++)
            {
                Array.Copy(dataBuffer, 0 + (192 * i), Index, 0, 4);

                Array.Copy(dataBuffer, 4 + (192 * i), FileName, 0, 161);

                Array.Copy(dataBuffer, 168 + (192 * i), FilenameCRC, 0, 4);

                Array.Copy(dataBuffer, 172 + (192 * i), DiskSize, 0, 4);

                Array.Copy(dataBuffer, 176 + (192 * i), DataSize, 0, 4);

                Array.Copy(dataBuffer, 180 + (192 * i), CRC, 0, 4);

                Array.Copy(dataBuffer, 184 + (192 * i), FileOffset, 0, 4);

                Array.Copy(dataBuffer, 188 + (192 * i), PackedType, 0, 1);

                string tempFileName = Encoding.Default.GetString(FileName).Replace("\0", "");

                string fileName = tempFileName.Contains("type\"") ? tempFileName.Substring(0, tempFileName.IndexOf("type\"", StringComparison.Ordinal) - 1) : tempFileName;

                // Fail-safe to make sure the path is valid
                if (!CrcHelper.GetCrc32HashFromMemoryToByteArray(Encoding.Default.GetBytes(fileName)).SequenceEqual(FilenameCRC))
                {
                    for (int j = 1; j < fileName.Length; j++)
                    {
                        var newFileName = fileName.Substring(0, j);
                        if (
                            CrcHelper.GetCrc32HashFromMemoryToByteArray(Encoding.Default.GetBytes(newFileName)).Reverse().ToArray()
                                .SequenceEqual(FilenameCRC))
                        {
                            fileName = newFileName;
                            break;
                        }
                    }
                }

                // New drive point?
                //DPHelper.CheckIfContainsDrivePoint(fileName); TODO

                rtnList.Add(new IndexItem(BitConverter.ToInt32(Index, 0),
                    fileName,
                    System.Text.RegularExpressions.Regex.Replace(BitConverter.ToString(FilenameCRC), "-", ""),
                    BitConverter.ToInt32(DiskSize, 0),
                    BitConverter.ToInt32(DataSize, 0),
                    BitConverter.ToInt32(CRC, 0).ToString("X"),
                    BitConverter.ToInt32(FileOffset, 0),
                    Convert.ToInt32(PackedType[0]),
                    parentFile));
            }

            return rtnList;
        }

        /// <summary>
        /// Buils EIX and EPK files
        /// </summary>
        /// <param name="list">Deserialized list</param>
        /// <param name="packFilePath">Path to EPK file, including the name</param>
        /// <param name="unpackedFilesPath">Path to unpacked files</param>
        /// <param name="indexKey">Index XTEA key</param>
        /// <param name="packKey">Pack XTEA key</param>
        /// <param name="errorLogCallBack"></param>
        /// <param name="progressCallback">Callback if progress updates are needed</param>
        /// <returns></returns>
        public static bool BuildIndexAndPackFiles(
            List<IndexItem> list,
            string packFilePath,
            string unpackedFilesPath,
            byte[] indexKey,
            byte[] packKey,
            Action<ErrorItem> errorLogCallBack,
            Action<int, int> progressCallback,
            Action fatalErrorCallback)
        {
            using (var fStream = new MemoryStream())
            {
                packFilePath = packFilePath.Replace("\\", "/");
                // Check if directory exists
                string directoryPath = packFilePath.Substring(0, packFilePath.LastIndexOf("/"));
                Directory.CreateDirectory(directoryPath);

                // Create stream to EPK file
                var epkStream = new FileStream(EterHelper.ReplaceWithEpkExt(packFilePath), FileMode.Create);

                // Size from header for each file
                int decompressedSize = 0;
                int compressedSize = 0;
                uint encryptedSize = 0;

                // File counter to index files
                int indexCount = 0;

                // Progress variables
                double actionProgress = 0;

                // FileOffset holder (EPK stream's length)
                int fileOffset = 0;

                // Write first header to EIX file
                fStream.Write(ConstantsBase.EterFourCc, 0, ConstantsBase.EterFourCc.Length);
                fStream.Write(BitConverter.GetBytes(2), 0, 4);
                fStream.Write(BitConverter.GetBytes(list.Count), 0, 4);

                try
                {
                    foreach (IndexItem item in list)
                    {
                        // Loop through items
                        var fileName = new byte[161];
                        var fileNameCrc = new byte[4];

                        //Index item's structure (totalizing 192 bytes)
                        #region Byte Holders

                        var fileIndex = new byte[4];
                        var realDataSize = new byte[4];
                        var dataSize = new byte[4];
                        var dataCrc = new byte[4];
                        var dataOffset = new byte[4];
                        var padding3B = new byte[3];

                        #endregion

                        // Set all backslashs to forwards slashes
                        item.Filename = item.Filename.Replace('\\', '/');

                        // Get raw data
                        byte[] rawData = IOHelper.ReadFile(unpackedFilesPath + item.Filename);

                        // Header sizees
                        int encryptedFileSize = 0;
                        int compressedFileSize = 0;
                        int decompressedFileSize = 0;

                        // Set real data & decompressed size to raw data's lenth
                        realDataSize = BitConverter.GetBytes(rawData.Length);
                        decompressedFileSize = rawData.Length;

                        // Set fileoffset to actual stream's length
                        fileOffset = (int)epkStream.Length;

                        #region File Type Processing

                        // Switch through the 3 possible cases
                        switch (item.PackType)
                        {
                            case 0:
                                // Write data to EPK stream
                                epkStream.Write(rawData, 0, rawData.Length);

                                // Set data size equal to raw data since no compression nor encrypted occured
                                dataSize = BitConverter.GetBytes(rawData.Length);
                                break;
                            case 1:
                            case 2:
                                // Compress data
                                byte[] compressedDataBuffer = LzoHelper.CompressData(rawData.Length, rawData);

                                // Create buffer to hold header + compressedData 
                                byte[] compressedDataWithHeaderBuffer = new byte[compressedDataBuffer.Length + 4];

                                // Copy header and compressedData to previously created buffer
                                Array.Copy(ConstantsBase.LzoFourCc, 0, compressedDataWithHeaderBuffer, 0, 4);
                                Array.Copy(compressedDataBuffer, 0, compressedDataWithHeaderBuffer, 4, compressedDataBuffer.Length);

                                // Set dataSize to compressedSize (since it assumes it's type 1)
                                dataSize = BitConverter.GetBytes(compressedDataWithHeaderBuffer.Length + 16);

                                // Set compressedSize
                                compressedFileSize = compressedDataBuffer.Length;

                                // If type 2
                                if (item.PackType == 2)
                                {
                                    // Get encrypted size (ALWAYS the upper multiple)
                                    encryptedFileSize = GetUpperMultiple(compressedDataWithHeaderBuffer.Length);

                                    // Resize data to fit encryptedSize
                                    Array.Resize(ref compressedDataWithHeaderBuffer, encryptedFileSize);

                                    // Encrypt Data
                                    Xtea.Encrypt2(ref compressedDataWithHeaderBuffer, packKey);

                                    // Set dataSize to encryptedData + header
                                    dataSize = BitConverter.GetBytes(compressedDataWithHeaderBuffer.Length + 16);
                                }

                                // Write header of file to EPK stream
                                epkStream.Write(ConstantsBase.LzoFourCc, 0, 4);
                                epkStream.Write(BitConverter.GetBytes(encryptedFileSize), 0, 4);
                                epkStream.Write(BitConverter.GetBytes(compressedFileSize), 0, 4);
                                epkStream.Write(BitConverter.GetBytes(decompressedFileSize), 0, 4);

                                // Write actual data
                                epkStream.Write(compressedDataWithHeaderBuffer, 0, compressedDataWithHeaderBuffer.Length);
                                break;
                        }

                        #endregion

                        #region Building index file

                        // Check if string replacment is needed
                        string virtualPathFile = DrivePointManager.InsertDrivePoints(item.Filename);

                        // Populate byte[] with data
                        fileIndex = BitConverter.GetBytes(item.Index);
                        byte[] fileNameTemp = Encoding.Default.GetBytes(virtualPathFile);
                        fileNameCrc = CrcHelper.GetCrc32HashFromMemoryToByteArray(Encoding.Default.GetBytes(virtualPathFile));
                        realDataSize = (realDataSize == null) ? BitConverter.GetBytes(item.DiskSize) : realDataSize;
                        dataSize = (dataSize == null) ? BitConverter.GetBytes(item.Size) : dataSize;
                        dataCrc = CrcHelper.GetCrc32HashToByteArray(unpackedFilesPath + item.Filename);
                        dataOffset = BitConverter.GetBytes(fileOffset);
                        var compressedType = (byte)item.PackType;

                        // Check if filename buffer is expectedSize
                        if (fileNameTemp.Length != 161)
                        {
                            Array.Copy(fileNameTemp, 0, fileName, 0, fileNameTemp.Length);
                        }

                        // Write data to EIX's stream
                        fStream.Write(fileIndex, 0, fileIndex.Length);
                        fStream.Write(fileName, 0, 161);
                        fStream.Write(padding3B, 0, padding3B.Length);
                        fStream.Write(fileNameCrc.Reverse().ToArray(), 0, fileNameCrc.Length);
                        fStream.Write(realDataSize, 0, realDataSize.Length);
                        fStream.Write(dataSize, 0, dataSize.Length);
                        fStream.Write(dataCrc.Reverse().ToArray(), 0, dataCrc.Length);
                        fStream.Write(dataOffset, 0, dataOffset.Length);
                        fStream.WriteByte(compressedType);
                        fStream.Write(padding3B, 0, padding3B.Length);

                        indexCount++;
                        #endregion

                        // Update progress
                        actionProgress = (indexCount / (double)list.Count * 100.0);
                        progressCallback(0, (int)actionProgress);
                    }
                }
                catch (Exception ex)
                {
                    //Logger.LogExceptioToFile(ex.ToString()); TODO
                    fatalErrorCallback();
                    throw;
                }

                // Assign current stream's lenght to decmopressedSize
                decompressedSize = (int)fStream.Length;

                // Buffer to hold compressedData
                byte[] compressedData = LzoHelper.CompressData(decompressedSize, fStream.ToArray());

                // Buffer with compressedData + MCOZ header
                byte[] compressedDataWithHeader = new byte[compressedData.Length + 4];

                // Copy Header to buffer
                Array.Copy(ConstantsBase.LzoFourCc, 0, compressedDataWithHeader, 0, 4);

                // Copy data to buffer
                Array.Copy(compressedData, 0, compressedDataWithHeader, 4, compressedData.Length);

                // Save compressedSize
                compressedSize = compressedData.Length;

                // Save encryptedSize (round to upper multiple)
                encryptedSize = (uint)GetUpperMultiple(compressedSize + 4);

                // Resize array to fit new size
                Array.Resize(ref compressedDataWithHeader, (int)encryptedSize);

                // Encrypt data
                Xtea.Encrypt2(ref compressedDataWithHeader, indexKey);

                // Create buffer to hold final data + header
                var outputFileBuffer = new byte[compressedDataWithHeader.Length + 16];

                // Copy header to buffer
                Array.Copy(ConstantsBase.LzoFourCc, 0, outputFileBuffer, 0, 4);
                Array.Copy(BitConverter.GetBytes(encryptedSize), 0, outputFileBuffer, 4, 4);
                Array.Copy(BitConverter.GetBytes(compressedSize), 0, outputFileBuffer, 8, 4);
                Array.Copy(BitConverter.GetBytes(decompressedSize), 0, outputFileBuffer, 12, 4);

                // Copy data to buffer
                Array.Copy(compressedDataWithHeader, 0, outputFileBuffer, 16, compressedDataWithHeader.Length);

                // Close stream
                epkStream.Close();
                epkStream.Dispose();

                // Save file
                File.WriteAllBytes(EterHelper.ReplaceWithEixExt(packFilePath), outputFileBuffer);

                return true;
            }

        }

        #region Helper Methods

        /// <summary>
        /// Gets next multiple of 8
        /// </summary>
        /// <param name="number">Number to be checked</param>
        /// <returns></returns>
        private static int GetUpperMultiple(int number)
        {
            var v = (int)Math.Ceiling((number / (double)8)) * 8;
            return v;
        }

        /// <summary>
        /// Simple method to compare checksums
        /// </summary>
        /// <param name="toComputeData">Actual hash</param>
        /// <param name="expectedHash">To be expetected hash</param>
        /// <returns></returns>
        private static bool CompareCrcHashes(byte[] toComputeData, string expectedHash)
        {
            return (int.Parse(CrcHelper.GetCrc32HashFromMemoryToString(toComputeData).ToLower(), System.Globalization.NumberStyles.HexNumber) == int.Parse(expectedHash.ToLower(), System.Globalization.NumberStyles.HexNumber));
        }

        #endregion
    }
}
