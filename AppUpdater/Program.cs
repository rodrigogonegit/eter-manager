using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUpdater
{
    class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            if (File.Exists("updateLog.txt"))
                File.Delete("updateLog.txt");

            // Log time
            Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            string file = "tmp/update.zip";

            // Check file
            if (!File.Exists(file))
            {
                Log($"File {file} could not be reached!");
                return;
            }
    
            try
            {
                Process process;

                // If arg is not specified, then resort to default process name
                if (args.Length < 1)
                {
                    Log("PID not specified. Trying to find by name...");
                    // If PID not specified, search by name
                    process = Process.GetProcessesByName("EterManager").FirstOrDefault();

                    // Might be null since it may be ran independently
                }
                else
                {
                    Log("PID received as argument. Looking it up...");

                    // If PID specified, get by PID
                    int pid;
                    int.TryParse(args[0], out pid);
                    process = Process.GetProcesses().FirstOrDefault(x => x.Id == pid);
                }

                // Kill if found
                if (process != null && !process.HasExited)
                {
                    Log($"Found target process (PID: {process.Id}. Killing it...");
                    process.Kill();
                }

                Log($"Checking local files...");

                // Open zip file
                ZipArchive archive = ZipFile.OpenRead(file);

                // Delete if needed
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (File.Exists(entry.FullName))
                    {
                        Log($"Deleting {entry.FullName}");
                        File.Delete(entry.FullName);
                    }
                        
                }

                // Target dir
                var targetDir = Directory.GetCurrentDirectory();

                Log($"Extracting update package to {targetDir}...");

                // Extract
                archive.ExtractToDirectory(targetDir);

                Log("Extracted all files!");

                archive.Dispose();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        static void Log(string message)
        {
            File.AppendAllText("updateLog.txt", $"[*] {message}" + Environment.NewLine);
        }
    }
}
