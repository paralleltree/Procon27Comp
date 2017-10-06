using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Procon27Comp.Components;
using Procon27Comp.Internal;
using Procon27Comp.Solvers;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options(args);
            var puzzle = Puzzle.ReadFromData(File.ReadAllText(opts.RemainingArgs[0]).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

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
            reduced.DumpToImage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "reduced.png"));

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

            var list = new List<Puzzle>();
            list.Add(reduce(puzzle, 3));
            list.Add(reduce(puzzle, 2));

            var tokenSource = new System.Threading.CancellationTokenSource();
            var tasks = list.Select(p => Task.Run(() =>
            {
                var solver = new StupidSolver(p, tokenSource.Token);
                solver.Solve();
                return solver.Solutions.FirstOrDefault();
            })).ToArray();

            // 探索を打ち切ってnullを返す場合は使えないので実装変えてね☆
            int completedIndex = Task.WaitAny(tasks, 1000 * 60 * 7);
            Solution result = completedIndex == -1 ? null : tasks[completedIndex].Result;
            tokenSource.Cancel();
#endif

            if (result == null)
            {
                Console.WriteLine("Solution not found ('>_<)...");
            }
            else
            {
                string outputDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string imagePath = Path.Combine(outputDir, "out.png");
                result.DumpToImage(imagePath);
                result.DumpToText(Path.Combine(outputDir, "out.txt"));
                System.Diagnostics.Process.Start(imagePath);
            }

            Console.WriteLine("Done! ({0})", DateTime.Now - started);
            Console.WriteLine("何か押してね (/>_<)/");
            if (!opts.IsBatchMode) Console.ReadKey();
        }
    }

    /// <summary>
    /// 実行時のコマンドライン引数により指定されるオプションを表します。
    /// </summary>
    class Options
    {
        /// <summary>
        /// コマンドライン引数を解析した結果残った引数を取得します。
        /// </summary>
        public string[] RemainingArgs { get; private set; }

        /// <summary>
        /// バッチモードで起動されているかどうかを取得します。
        /// </summary>
        public bool IsBatchMode { get; private set; }

        /// <summary>
        /// コマンドライン引数を解析してオプションを設定します。
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        public Options(string[] args)
        {
            RemainingArgs = args.TakeWhile(p => p != "--").Where((p, i) =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(p, "(?<=^-)[a-z]$");
                if (!match.Success) return true;

                bool isOption = true;
                switch (match.Value)
                {
                    case "b":
                        IsBatchMode = true;
                        break;
                    default:
                        isOption = false;
                        break;
                }
                return !isOption;
            }).Concat(args.SkipWhile(p => p != "--").Skip(1)).ToArray();
        }
    }
}
