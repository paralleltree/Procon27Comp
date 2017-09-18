using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Procon27Comp.Components;
using Procon27Comp.Solvers;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            var puzzle = Puzzle.ReadFromData(File.ReadAllText(args[0]).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            Puzzle reduced = puzzle;
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(2);
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(1);
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(2);
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(1);
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(2);
            for (int i = 0; i < 10; i++) reduced = reduced.ReduceByEdge(1);

            var solver = new StupidSolver(reduced);
            solver.Solve();
            foreach (var s in solver.Solutions)
            {
                s.DumpToImage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png"));
            }
        }
    }
}
