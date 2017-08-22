using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vertesaur;
using Vertesaur.PolygonOperation;
using Procon27Comp.Internal;
using Procon27Comp.Components;
using Procon27Comp.Solvers;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            LibTest();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Documents\Visual Studio 2015\Projects\Procon27CompCorrector\Procon27CompCorrector\bin\Debug\multi_problem.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Documents\Visual Studio 2015\Projects\Procon27CompCorrector\Procon27CompCorrector\bin\Debug\test.txt").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\work\simple_sample.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\puzzle_example.txt").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\work\simple_sample_fix.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\problems\sisaku2_multiwaku\small.tera").Read();
            var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\problems\sisaku2_multiwaku\problem.txt").Read();
            var solver = new StupidSolver(puzzle);
            //var solution = new Solution(solver.Puzzle, new State(0));
            //solution.DumpToImage(@"C:\users\paltee\Desktop\out.png");
            solver.Solve();
            foreach (var s in solver.Solutions)
            {
                s.DumpToImage(string.Format(@"C:\Users\paltee\Desktop\out.png"));
            }
            var state = new State(1 << 4);
            var r = state.EnumerateUnusedPieceIndices();
            //var p = new PuzzleReader(@"C:\Users\paltee\Downloads\puzzle_example.txt").Read();
            //var s = new StupidSolver(p);
            //s.Solve();

        }

        static void LibTest()
        {
            var s1 = new Polygon2(new[]
            {
                new Point2(0,0),
                new Point2(10, 0),
                new Point2(10, 10),
                new Point2(0, 10)
            });

            var s2 = new Polygon2(new[]
            {
                new Point2(5, 5),
                new Point2(11, 5),
                new Point2(8, 2),
                new Point2(18, 5),
                new Point2(18, 8),
                new Point2(5, 8)
            });

            var o = (Polygon2)new PolygonIntersectionOperation().Intersect(s1, s2);
            double a = o.GetArea();

            var sq1 = new Polygon2(new[]
            {
                new Point2(0,0),
                new Point2(10, 0),
                new Point2(10, 10),
                new Point2(0, 10)
            });

            var sq2 = new Polygon2(new[]
            {
                new Point2(1, 1),
                new Point2(3, 1),
                new Point2(3, 3),
                new Point2(-2, 3)
            });

            var poly = (Polygon2)PolygonCalculation.Difference(sq2, sq1);

            var gp1 = new GpcWrapper.Polygon();
            gp1.AddContour(new GpcWrapper.VertexList(sq1.Single().Select(p => new System.Drawing.PointF((float)p.X, (float)p.Y)).ToArray()), false);
            var gp2 = new GpcWrapper.Polygon();
            gp2.AddContour(new GpcWrapper.VertexList(sq2.Single().Select(p => new System.Drawing.PointF((float)p.X, (float)p.Y)).ToArray()), false);
            var d = gp2.Clip(GpcWrapper.GpcOperation.Intersection, gp1);
        }
    }
}
