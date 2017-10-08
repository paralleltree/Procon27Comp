using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

using Procon27Comp.Components;
using Procon27Comp.Internal;

namespace Procon27Comp.Solvers
{
    internal static class VertexPreprocessor
    {
        /// <summary>
        /// パズルのピースを削減する前処理を行います。
        /// 繰り返し適用することでより多くのピースを削減できる場合があります。
        /// </summary>
        /// <param name="puzzle">前処理を行う<see cref="Puzzle"/></param>
        /// <returns>処理を行ったピースを格納した<see cref="Puzzle"/></returns>
        public static Puzzle ReduceByEdge(this Puzzle puzzle)
        {
            var primeEdges = puzzle.Pieces
                .SelectMany((p, i) => p.Vertexes.Zip(p.Vertexes.Skip(1).Concat(p.Vertexes.Take(1)), (q, r) => (r.Location - q.Location) / 10).Where(q => q.X != 0 && q.Y != 0).Select(q =>
                {
                    int x = (int)Math.Abs(q.X);
                    int y = (int)Math.Abs(q.Y);
                    int gcd = GetGcd(x, y);
                    return new { PieceIndex = i, PrimeEdgeLengthSquared = (x / gcd) * (x / gcd) + (y / gcd) * (y / gcd) };// Tuple.Create((int)q.X / gcd, (int)q.Y / gcd) };
                }))
                .GroupBy(p => p.PrimeEdgeLengthSquared)
                .ToDictionary(p => p.Key, p => p.ToList());

            var targets = primeEdges.Where(p => p.Value.Count == 2);

            var list = new List<Piece>(puzzle.Pieces);
            int[] parent = new int[list.Count];
            for (int i = 0; i < list.Count; i++) parent[i] = -1;
            foreach (var item in targets)
            {
                int pi1 = item.Value[0].PieceIndex;
                if (parent[pi1] != -1) pi1 = parent[pi1];
                int pi2 = item.Value[1].PieceIndex;
                if (parent[pi2] != -1) pi2 = parent[pi2];

                var merged = new Piece[] { list[pi2], list[pi2].Flip() }
                    .SelectMany(p => Enumerable.Range(0, 4).SelectMany(q => Merge(list[pi1], p.Rotate(q * 90 * (float)Math.PI / 180), item.Key))).ToList();
                if (merged.Count != 1) continue;
                list[pi1] = merged.Single();
                parent[pi2] = pi1;
            }
            return new Puzzle(puzzle.Frames, list.Where((p, i) => parent[i] == -1).ToList());
        }

        private static IEnumerable<MergedPiece> Merge(Piece a, Piece b, int primeLengthSquared)
        {
            var reversedA = a.Vertexes.Reverse().ToList();
            var vlistB = b.Vertexes.ToList();
            for (int i = 0; i < reversedA.Count; i++)
            {
                for (int j = 0; j < b.Vertexes.Count; j++)
                {
                    int k = 0;
                    Vector2 vecA = reversedA[(i + k + 1) % reversedA.Count].Location - reversedA[(i + k) % reversedA.Count].Location;
                    Vector2 vecB = vlistB[(j + k + 1) % vlistB.Count].Location - vlistB[(j + k) % vlistB.Count].Location;
                    if (vecA != vecB || vecA.GetReducedLengthSquared() != primeLengthSquared) continue;

                    Func<Vector2, Vector2, MergedPiece> transform = (avec, bvec) =>
                    {
                        var nodeA = reversedA[i].Location;
                        var nodeB = vlistB[j].Location;
                        float angle = (float)VectorHelper.CalcAngle(avec, bvec);
                        if (Math.Abs(angle % (Math.PI / 2)) > 1E-4) return null; // 90度以外はそもそも回転後に頂点が格子状にないので飛ばす
                        bool isBLeft = Vector3.Cross(new Vector3(avec, 0), new Vector3(bvec, 0)).Z > 0;
                        var transformed = b.Offset(-nodeB.X, -nodeB.Y).Rotate(isBLeft ? -angle : angle).Offset(nodeA.X, nodeA.Y);
                        if ((PolygonCalculation.Intersect(transformed.GetPolygon(), a.GetPolygon())).GetArea() > 0)
                            return null;
                        var union = PolygonCalculation.Union(a.GetPolygon(), transformed.GetPolygon());
                        if (union.Count > 1) return null;
                        return new MergedPiece(union.Single().Select(p => new Vector2((float)p.X, (float)p.Y)),
                            new Piece[] { a, transformed });
                    };
                    var res = transform(vecA, vecB);
                    if (res != null) yield return res;
                }
            }
        }

        private static int GetGcd(int a, int b)
        {
            if (a < b)
            {
                int t = a;
                a = b;
                b = t;
            }
            while (b > 0)
            {
                int r = a % b;
                a = b;
                b = r;
            }
            return a;
        }

        private static int GetReducedLengthSquared(this Vector2 vec)
        {
            int x = (int)Math.Abs(vec.X);
            int y = (int)Math.Abs(vec.Y);
            int gcd = GetGcd(x, y);
            return (x / gcd) * (x / gcd) + (y / gcd) * (y / gcd);
        }
    }
}
