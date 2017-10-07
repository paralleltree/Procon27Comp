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
            string monitorTarget = Path.GetFullPath(args[0]);
            string launchTarget = string.Format("\"{0}\"", args.Skip(1).First());
            string launchArgs = string.Join(" ", args.Skip(2).Select(p => "\"" + p + "\"").ToArray());

            var watcher = new FileSystemWatcher()
            {
                Path = Path.GetDirectoryName(monitorTarget),
                Filter = Path.GetFileName(monitorTarget)
            };
            watcher.Changed += (s, e) => System.Diagnostics.Process.Start(launchTarget, string.Format("{0} {1}", launchArgs, string.Format("\"{0}\"", monitorTarget)));
            watcher.EnableRaisingEvents = true;
            watcher.WaitForChanged(WatcherChangeTypes.Changed);
        }
    }
}
