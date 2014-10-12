using System.Collections.Generic;

namespace EterManager.Services.Abstract
{
    public interface IDrivePointManager
    {
        /// <summary>
        /// Holds reference to all the drive points
        /// </summary>
         Dictionary<string, string> DrivePoints { get; set; }

        /// <summary>
        /// Inserts virtual paths
        /// </summary>
        /// <param name="toCheck"></param>
        void InsertDrivePoints(ref string toCheck);

        /// <summary>
        /// Inserts virtual paths and returns the new string
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        string InsertDrivePoints(string toCheck);

        /// <summary>
        /// Converts the virtual paths into absolute paths
        /// </summary>
        /// <param name="toCheck"></param>
        void RemoveDrivePoints(ref string toCheck);

        /// <summary>
        /// Convertes the virtual paths into absolute paths and returns the new string
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        string RemoveDrivePoints(string toCheck);

        /// <summary>
        /// Checks if string contains any illegal characters and if they correspond to a virtual path (drive point)
        /// </summary>
        /// <param name="path"></param>
        void CheckIfContainsDrivePoint(string path);
    }
}