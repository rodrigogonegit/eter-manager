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
        public Dictionary<string, string> DrivePoints { get; set; }

        // Logger
        private readonly ILogger _logger = ((App) Application.Current).GetInstance<ILogger>();

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
                _logger.Information("NEW_DRIVE_POINT_ADDED", null, drivePoint);

            }
        }
    }
}
