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
        static void Main(string[] args)
        {
            string file = "tmp/update.zip";

            // Check file
            if (!File.Exists(file))
                return;

            try
            {
                Process process;

                // If arg is not specified, then resort to default process name
                if (args.Length < 1)
                {
                    // If PID not specified, search by name
                    process = Process.GetProcessesByName("EterManager").FirstOrDefault();

                    // Might be null since it may be ran independently
                }
                else
                {
                    // If PID specified, get by PID
                    int pid;
                    int.TryParse(args[0], out pid);
                    process = Process.GetProcessById(pid);
                }

                // Kill if found
                process?.Kill();

                // Open zip file
                ZipArchive archive = ZipFile.OpenRead(file);

                // Delete if needed
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (File.Exists("tmp/" + entry.FullName))
                        File.Delete("tmp/" + entry.FullName);
                }

                // Extract
                archive.ExtractToDirectory("tmp/");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
