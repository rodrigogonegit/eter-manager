using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using EterManager.Services.Abstract;

namespace EterManager.Services.Concrete
{
    class DrivePointManager : IDrivePointManager
    {
        /// <summary>
        /// Holds a list of all the drive points
        /// </summary>
        public Dictionary<string, string> DrivePoints { get; set; }

        // WindowLog
        private readonly IWindowLog _windowLog = ((App) Application.Current).GetInstance<IWindowLog>();

        /// <summary>
        /// Inserts a new drive point to the dictionary
        /// </summary>
        /// <param name="toCheck"></param>
        public void InsertDrivePoints(ref string toCheck)
        {
            if (DrivePoints == null)
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();

            foreach (var dictItem in DrivePoints)
            {
                if (toCheck.Contains(dictItem.Key) && !toCheck.Contains(dictItem.Value))
                {
                    toCheck = System.Text.RegularExpressions.Regex.Replace(toCheck, dictItem.Key, dictItem.Value);
                }
            }
        }

        /// <summary>
        /// Inserts a new drive point to the dictionary
        /// </summary>
        /// <param name="toCheck"></param>
        public string InsertDrivePoints(string toCheck)
        {
            if (DrivePoints == null)
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();

            foreach (var dictItem in DrivePoints)
            {
                if (toCheck.Contains(dictItem.Key) && !toCheck.Contains(dictItem.Value))
                {
                    toCheck = System.Text.RegularExpressions.Regex.Replace(toCheck, dictItem.Key, dictItem.Value);
                }
            }
            return toCheck;
        }

        /// <summary>
        /// Removes a drive point from the path
        /// </summary>
        /// <param name="toCheck"></param>
        public void RemoveDrivePoints(ref string toCheck)
        {
            if (DrivePoints == null)
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();

            foreach (var dictItem in DrivePoints)
            {
                if (toCheck.Contains(dictItem.Value))
                {
                    toCheck = System.Text.RegularExpressions.Regex.Replace(toCheck, dictItem.Value, dictItem.Key);
                }
            }
        }

        /// <summary>
        /// Removes a drive point from path
        /// </summary>
        /// <param name="toCheck"></param>
        public string RemoveDrivePoints(string toCheck)
        {
            if (DrivePoints == null)
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();

            foreach (var dictItem in DrivePoints)
            {
                if (toCheck.Contains(dictItem.Value))
                {
                    toCheck = System.Text.RegularExpressions.Regex.Replace(toCheck, dictItem.Value, dictItem.Key);
                }
            }
            return toCheck;
        }

        /// <summary>
        /// Checks if path contains a drive point
        /// </summary>
        /// <param name="path"></param>
        public void CheckIfContainsDrivePoint(string path)
        {
            if (DrivePoints == null)
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();

            path = path.Replace("\\", "/");

            if (path.Contains(":"))
            {
                string[] folders = Regex.Split(path, "/");
                string driveName = path.Substring(0, 2);

                if (DrivePoints.ContainsKey(folders[1].Trim()))
                    return;

                string drivePoint = String.Format("{0}={1}/{2}", folders[1], driveName, folders[1]);
                DataAccessLayer.DrivePointsDal.AddDrivePoint(drivePoint);
                DrivePoints = DataAccessLayer.DrivePointsDal.GetDrivePoints();
                _windowLog.Information("NEW_DRIVE_POINT_ADDED", null, drivePoint);

            }
        }
    }
}
