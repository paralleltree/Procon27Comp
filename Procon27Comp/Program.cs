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

            DateTime started = DateTime.Now;

#if DEBUG
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

            Solution result = solver.Solutions.Count == 0 ? null : solver.Solutions.Single();

#else

            Func<Puzzle, int, Puzzle> reduce = (p, min) =>
            {
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 3; j >= min; j--)
                    {
                        for (int i = 0; i < 3; i++) p = p.ReduceByEdge(j);
                    }
                }
                return p;
            };

            var reduced = new List<Puzzle>();
            reduced.Add(reduce(puzzle, 3));
            reduced.Add(reduce(puzzle, 2));

            var tasks = reduced.Select(p => Task.Run(() =>
            {
                var solver = new StupidSolver(p);
                solver.Solve();
                return solver.Solutions.FirstOrDefault();
            })).ToArray();

            // 探索を打ち切ってnullを返す場合は使えないので実装変えてね☆
            int completedIndex = Task.WaitAny(tasks, 1000 * 60 * 7);
            Solution result = completedIndex == -1 ? null : tasks[completedIndex].Result;
#endif

            if (result == null)
            {
                Console.WriteLine("Solution not found ('>_<)...");
            }
            else
            {
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png");
                result.DumpToImage(outputPath);
                System.Diagnostics.Process.Start(outputPath);
            }

            Console.WriteLine("Done! ({0})", DateTime.Now - started);
            Console.WriteLine("何か押してね (/>_<)/");
            Console.ReadKey();
        }
    }
}
