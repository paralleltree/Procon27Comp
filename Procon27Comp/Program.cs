using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon27Comp.Components;
using Procon27Comp.Solvers;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            var puzzle = new PuzzleReader(args[0]).Read();
            var solver = new StupidSolver(puzzle);
            solver.Solve();
            foreach (var s in solver.Solutions)
            {
                s.DumpToImage(string.Format("out.png"));
            }
        }
    }
}
