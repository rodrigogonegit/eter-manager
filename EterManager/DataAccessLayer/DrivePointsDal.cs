using System;
using System.Collections.Generic;
using System.IO;

namespace EterManager.DataAccessLayer
{
    class DrivePointsDal
    {
        /// <summary>
        /// Return Dictionary containning correspondencis between actual paths and drive points
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetDrivePoints()
        {
            var toRtn = new Dictionary<string, string>();

            using (var sReader = new StreamReader("AppData/dp.settings"))
            {
                string currentLine;

                while ((currentLine = sReader.ReadLine()) != null)
                {
                    if (currentLine.Trim() == "")
                        continue;

                    string[] tokens = currentLine.Split('=');

                    toRtn.Add(tokens[0], tokens[1]);
                }
            }
            return toRtn;
        }

        /// <summary>
        /// Adds new drive point to settings file
        /// </summary>
        /// <param name="toAdd"></param>
        public static void AddDrivePoint(string toAdd)
        {
            using (var sWritter = File.AppendText("AppData/dp.settings"))
            {
                sWritter.WriteLine(Environment.NewLine + toAdd);
            }
        }
    }
}
