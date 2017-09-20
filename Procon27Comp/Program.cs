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
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 4; i++) reduced = reduced.ReduceByEdge(2);
                for (int i = 0; i < 2; i++) reduced = reduced.ReduceByEdge(1);
            }

            var solver = new StupidSolver(reduced);
            solver.Solve();

            var solution = solver.Solutions.Single();
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png");
            solution.DumpToImage(outputPath);
            System.Diagnostics.Process.Start(outputPath);
        }
    }
}
