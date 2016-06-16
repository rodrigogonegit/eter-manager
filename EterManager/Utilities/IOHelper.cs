using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using EterManager.Services;

namespace EterManager.Utilities
{
    public static class IOHelper
    {
        /// <summary>
        /// Reference to the logger
        /// </summary>
        private static IWindowLog _windowLog = ((App)Application.Current).GetInstance<IWindowLog>();

        /// <summary>
        /// Read input file to byte array using a Filestream
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

        /// <summary>
        /// Reads specified chunk of data from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataOffset"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        public static byte[] ReadFileOffsetToLength(string filePath, int dataOffset, int dataLength)
        {
            using (var b = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                //Data and file values
                int fileLength = (int)b.BaseStream.Length;
                int count = 0;
                var file = new byte[dataLength];

                //Move to specified offset
                b.BaseStream.Seek(dataOffset, SeekOrigin.Begin);

                //Slow loop through the bytes.
                while (dataOffset < fileLength && count < dataLength)
                {
                    file[count] = b.ReadByte();

                    //Increment the variables.
                    dataOffset++;
                    count++;
                }

                return file;
            }
        }

        /// <summary>
        /// Checks if file is locked
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Overload methods which takes a string as the path instead of the FileInfo object
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            var file = new FileInfo(filePath);

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Checks if directory if accessisable
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static bool CanAccessFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads offset from array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int ReadIntFromArray(byte[] array, int offset, int length)
        {
            var temp = new byte[length];
            Array.Copy(array, offset, temp, 0, length);
            return BitConverter.ToInt32(temp, 0);
        }

        /// <summary>
        /// Gets total size in bytes of a directory's files
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static long GetTotalFileSizeOfDirectory(string dir)
        {
            return GetAllFilesFromDir(dir).Sum(x => x.Length);
        }

        /// <summary>
        /// Deletes everything from directory
        /// </summary>
        /// <param name="dirPath"></param>
        public static void CleanDirectory(string dirPath)
        {
            foreach (var file in GetAllFilesFromDir(dirPath))
                File.Delete(file.FullName);
        }

        /// <summary>
        /// Gets all files from directory (recursive)
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static List<FileInfo> GetAllFilesFromDir(string dirPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(dirPath);

                List<FileInfo> returnList = dirInfo.GetFiles().ToList();

                foreach (var dir in dirInfo.GetDirectories())
                {
                    ProcessDir(dir, ref returnList);
                }

                return returnList;
            }
            catch (UnauthorizedAccessException ex)
            {
                _windowLog.Error("ERROR_READING_PATH", null, dirPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                _windowLog.Error("DIR_NOT_FOUND", null, dirPath);
            }

            return new List<FileInfo>();
        }

        /// <summary>
        /// Processes directory
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="returnList"></param>
        private static void ProcessDir(DirectoryInfo dirInfo, ref List<FileInfo> returnList)
        {
            returnList.AddRange(dirInfo.GetFiles());

            foreach (var dir in dirInfo.GetDirectories())
            {
                ProcessDir(dir, ref returnList);
            }
        }

        /// <summary>
        /// Returns a list of all the first layer directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<DirectoryInfo> GetAllFirstDirectories(string path)
        {
            // Return list
            return new DirectoryInfo(path).GetDirectories().ToList();
        }
    }
}
