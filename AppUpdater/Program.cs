using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Process process;

                if (args.Length < 1)
                {
                    // If PID not specified, search by name
                    process = Process.GetProcessesByName("EterManager")[0];
                }
                else
                {
                    // If PID specified, get by PID
                    int pid;
                    int.TryParse(args[0], out pid);
                    process = Process.GetProcessById(pid);
                }

                process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not terminate the target process! Please manually close the main application and try again.");
                Console.WriteLine(ex.ToString());
            }
            

        }
    }
}
