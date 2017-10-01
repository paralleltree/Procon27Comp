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

            Console.WriteLine("Preprocessing...");

            Puzzle reduced = puzzle;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 4; i++) reduced = reduced.ReduceByEdge(3);
                for (int i = 0; i < 4; i++) reduced = reduced.ReduceByEdge(2);
            }

            Console.WriteLine("Pieces count: {0} -> {1}", puzzle.Pieces.Count, reduced.Pieces.Count);
            Console.WriteLine("Solving...");

            var solver = new StupidSolver(reduced);
            solver.Solve();

            if (solver.Solutions.Count == 0)
            {
                Console.WriteLine("Solution not found ('>_<)...");
            }
            else
            {
                var solution = solver.Solutions.Single();
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png");
                solution.DumpToImage(outputPath);
                System.Diagnostics.Process.Start(outputPath);
            }
            Console.WriteLine("Done!");
        }
    }
}
