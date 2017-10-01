﻿using System;
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

            write_file(reduced[0], "prep3.txt");
            write_file(reduced[1], "prep2.txt");

            /*

            var tasks = reduced.Select(p => Task.Run(() =>
            {
                var solver = new StupidSolver(p);
                solver.Solve();
                return solver.Solutions.FirstOrDefault();
            })).ToArray();

            // 探索を打ち切ってnullを返す場合は使えないので実装変えてね☆
            Solution result = tasks[Task.WaitAny(tasks)].Result;
            */
#endif
            /*
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
            */
        }

        static void write_file(Puzzle reduced, String file_name)
        {

            using (StreamWriter w = new StreamWriter(file_name))
            {
                w.WriteLine("{0}", reduced.Pieces.Count());
                foreach (Piece piece in reduced.Pieces)
                {
                    w.Write("{0}", piece.Vertexes.Count());
                    foreach (Vertex pv in piece.Vertexes)
                    {
                        w.Write(" {0} {1}", pv.X / 10, pv.Y / 10);
                    }
                    w.Write("\n");
                }
                w.Write("{0}", reduced.Frames[0].Vertexes.Count());
                foreach (Vertex pv in reduced.Frames[0].Vertexes)
                {
                    w.Write(" {0} {1}", pv.X / 10, pv.Y / 10);
                }
                w.Write("\n");
            }

        }
    }
}
