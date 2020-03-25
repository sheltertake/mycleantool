using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MyCleanTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ScanDir(@"c:\D3MS\");
            ScanDir(@"c:\IOD\");
            ScanDir(@"c:\UefaCom\");
            ScanDir(@"c:\RS\");

            ScanDir(@"c:\GIT\");

            ScanDir(@"c:\FAME2\");
            ScanDir(@"c:\Unity\");

            ScanDir(@"c:\VSTS\");
        }

        private static void ScanDir(string path)
        {
            var sp = Stopwatch.StartNew();
            Console.WriteLine("Free space: " + GetTotalFreeSpace(path.Substring(0,1)));
            Console.WriteLine(path);
            var all = Directory.GetDirectories(path,
                "*",
                SearchOption.AllDirectories);

            var node_modules = all.Where(s => s.EndsWith("node_modules") && !s.Contains("\\node_modules\\"));
            Console.WriteLine("Node modules: " + node_modules.Count());

            var bins = all.Where(s => s.EndsWith("bin") && !s.Contains("\\node_modules\\"));
            var bins_validated = new List<string>();
            foreach (var dir in bins)
            {
                var csproj = Directory.GetFiles(dir + "\\..\\", "*.csproj");
                var vbproj = Directory.GetFiles(dir + "\\..\\", "*.vbproj");
                if (csproj.Any() || vbproj.Any())
                {
                    bins_validated.Add(dir);
                }
            }
            Console.WriteLine("bin: " + bins.Count() + " csproj:" + bins_validated.Count);

            var objs = all.Where(s => s.EndsWith("obj"));
            var objs_validated = new List<string>();
            foreach (var dir in objs)
            {
                var csproj = Directory.GetFiles(dir + "\\..\\", "*.csproj");
                var vbproj = Directory.GetFiles(dir + "\\..\\", "*.vbproj");
                if (csproj.Any() || vbproj.Any())
                {
                    objs_validated.Add(dir);
                }   
            }
            Console.WriteLine("Obj: " + objs.Count() + " csproj:" + bins_validated.Count);

            var packages = all.Where(s => s.EndsWith("packages"));
            Console.WriteLine("Packages: " + packages.Count());

            //log
            var dirtodelete = node_modules.Concat(bins_validated).Concat(objs_validated).Concat(packages);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory +  path.Replace("c:\\","").Replace("\\","") + ".txt", JsonConvert.SerializeObject(
                dirtodelete
                , Formatting.Indented));
            // Write hours, minutes and seconds.
            sp.Stop();
            Console.WriteLine("Time elapsed scan: {0:hh\\:mm\\:ss}", sp.Elapsed);
            //var Key = Console.ReadLine();
            //Console.WriteLine("You entered: " + Key + "");


            foreach (var directory in dirtodelete)
            {
                Console.WriteLine("Deleting " + directory);
                var spinner = Stopwatch.StartNew();
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting " + directory );
                    Console.WriteLine("Exception " + ex.Message);
                }
                spinner.Stop();
                Console.WriteLine("Deleted " + directory);
                Console.WriteLine("Time elapsed delete folder: {0:hh\\:mm\\:ss}", spinner.Elapsed);
                Console.WriteLine("Free space after deleted folder: " + GetTotalFreeSpace(path.Substring(0, 1)));
            }
            Console.WriteLine("Free space: " + GetTotalFreeSpace(path.Substring(0, 1)));
        }


        private static long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name.ToLower().Substring(0,1) == driveName)
                {
                    return (long) (drive.TotalFreeSpace / 1000 / 1048576D);
                }
            }
            return -1;
        }
}
}
