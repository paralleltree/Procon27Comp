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
            Console.WriteLine("Solving...");

            var solver = new StupidSolver(puzzle);
            solver.Solve();

            Solution result = solver.Solution;

#else

            var tokenSource = new System.Threading.CancellationTokenSource();

            var list = new List<StupidSolver>();
            list.Add(new StupidSolver(puzzle, tokenSource.Token) { InitialBeamWidth = 4 });

            var tasks = list.Select(p => Task.Run(() =>
            {
                p.Solve();
                return p.Solution;
            })).ToArray();

            // 探索を打ち切ってnullを返す場合は使えないので実装変えてね☆
            int completedIndex = Task.WaitAny(tasks, 1000 * 60 * 7);
            Solution result = completedIndex == -1 ? null : tasks[completedIndex].Result;
            tokenSource.Cancel();

            if (completedIndex != -1)
            {
                Console.WriteLine("Solver params:");
                Console.WriteLine("  InitialBeamWidth: {0}", list[completedIndex].InitialBeamWidth);
            }
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
