using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using Vertesaur;
using Vertesaur.PolygonOperation;

using Procon27Comp.Components;
using Procon27Comp.Solvers;
using Procon27Comp.Internal;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            new int[] { 2, 1, 2 }.CircularEqual(new int[] { 1, 2, 2 });
            //var pp = Puzzle.ReadFromData(new string[] { "8:5 7 1 6 5 4 5 0 2 6 0:3 0 0 4 4 0 5:5 2 5 0 5 5 0 5 8 2 8:3 6 2 0 7 0 0:5 6 5 0 0 0 13 9 2 9 5:4 0 0 4 0 4 5 0 3:8 5 1 5 0 7 0 7 3 0 3 0 0 2 0 2 1:4 0 0 3 0 3 3 0 3:9 11 0 11 2 13 2 13 0 16 0 16 10 0 10 0 3 4 0" }.ToList());
            //var pp = Puzzle.ReadFromData(new string[] { File.ReadAllText(@"C:\Users\paltee\Downloads\sample_data_txt.txt") }.ToList());
            //LibTest();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Documents\Visual Studio 2015\Projects\Procon27CompCorrector\Procon27CompCorrector\bin\Debug\multi_problem.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Documents\Visual Studio 2015\Projects\Procon27CompCorrector\Procon27CompCorrector\bin\Debug\test.txt").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\work\simple_sample.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\puzzle_example.txt").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\work\simple_sample_fix.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\problems\sisaku2_multiwaku\small.tera").Read();
            //var puzzle = new PuzzleReader(@"C:\Users\paltee\Downloads\problems\sisaku2_multiwaku\problem.txt").Read();
            //var puzzle = Puzzle.ReadFromData(new string[] { File.ReadAllText(@"C:\Users\paltee\Downloads\test1.txt") }.ToList());
            // C:\Users\paltee\Downloads\MakePuzzle\sample.txt
            var puzzle = Puzzle.ReadFromData(File.ReadAllText(args.Length == 0 ? @"C:\Users\paltee\Downloads\my_sample.txt" : args[0]).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            Action<Puzzle> dump = pz =>
            {
                using (var bmp = new Bitmap(1200, 800))
                {
                    GraphicsHelper.WorkWithGraphic(bmp, g =>
                    {
                        pz.Pieces.Select((p, i) => new { Index = i, Piece = p }).ToList()
                        .ForEach(p =>
                        {
                            p.Piece.GetPolygon().DrawToImage(g, Pens.Blue);
                            var centroid = p.Piece.GetPolygon().GetCentroid();
                            g.DrawString(p.Index.ToString(), new Font("MS Gothic", 10), Brushes.Brown, new PointF((float)centroid.X, (float)centroid.Y));
                        });
                    });
                    bmp.SaveAsPng(@"C:\users\paltee\Desktop\ans1.png");
                }
            };

            DateTime started = DateTime.Now;
            Console.WriteLine("Preprocessing...");

            var hint = puzzle.ApplyHint(ArrangementHintPreprocessor.ReadFromData("8:9 0 0 26 0 26 5 18 5 18 17 15 24 10 24 4 20 0 20:5 6 42 7 37 12 39 12 48 5 46:6 26 18 35 18 28 24 26 28 17 32 16 28:10 26 28 28 24 35 18 42 18 52 24 51 28 50 33 48 42 33 42 29 35:5 50 33 57 36 57 45 53 44 48 42:7 58 19 73 5 92 4 100 4 100 9 81 9 59 24:4 69 24 75 24 75 30 69 30:7 73 59 85 59 85 51 81 42 100 42 100 64 73 64"));

            Puzzle reduced = puzzle;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 4; i++) reduced = reduced.ReduceByEdge(3);
                //for (int i = 0; i < 2; i++) reduced = reduced.ReduceByEdge(1);
            }
            dump(puzzle);
            dump(reduced);

            Console.WriteLine("Done! ({0})", (DateTime.Now - started).ToString());
            Console.WriteLine("Pieces count: {0} -> {1}", puzzle.Pieces.Count, reduced.Pieces.Count);
            Console.WriteLine("Solving...");

            var solver = new StupidSolver(reduced);
            solver.Solve();

            var solution = solver.Solutions.Single();
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png");
            solution.DumpToImage(outputPath);
            System.Diagnostics.Process.Start(outputPath);
            //foreach (var s in solver.Solutions)
            //{
            //    s.DumpToImage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "out.png"));
            //}
            Console.WriteLine("Done! ({0})", solver.ExecutionTime);
            Console.ReadLine();
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
