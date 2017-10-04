using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Procon27Comp.WatchAndLaunch
{
    class Program
    {
        static void Main(string[] args)
        {
            string monitorTarget = args[0];
            string launchTarget = args[1];

            var watcher = new FileSystemWatcher()
            {
                Path = Path.GetDirectoryName(monitorTarget),
                Filter = Path.GetFileName(monitorTarget)
            };
            watcher.Changed += (s, e) => System.Diagnostics.Process.Start(launchTarget, monitorTarget);
            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
        }
    }
}
