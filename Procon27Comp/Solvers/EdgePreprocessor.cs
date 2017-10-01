﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Numerics = System.Numerics;
using Vertesaur;

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
        /// <param name="edgeCount">最低一致辺数</param>
        /// <returns>処理を行ったピースを格納した<see cref="Puzzle"/></returns>
        public static Puzzle ReduceByEdge(this Puzzle puzzle, int edgeCount)
        {
            var plist = new List<Piece>(puzzle.Pieces);
            bool[] used = new bool[plist.Count];
            for (int i = 0; i < plist.Count; i++)
            {
                if (used[i]) continue;
                var mergedPieces = new List<MergedPiece>();
                bool[] localUsed = new bool[plist.Count];
                for (int j = 0; j < plist.Count; j++)
                {
                    if (used[j] || i == j) continue;
                    var merged = Enumerable.Range(0, 4).SelectMany(p => Merge(plist[i], plist[j].Rotate(p * 90 * (float)Math.PI / 180), edgeCount)).ToList();
                    if (merged.Count > 0 && (merged.Count == 1 || merged.Skip(1).All(p => p.GetPolygon().SpatiallyEqual(merged[0].GetPolygon()))))
                    {
                        mergedPieces.Add(merged.First());
                        localUsed[j] = true;
                    }
                }

                if (mergedPieces.Count == 0) continue;
                var mergedpoly = mergedPieces.Select(p => p.GetPolygon());
                var union = mergedpoly.Aggregate((p, q) => (Polygon2)PolygonCalculation.Union(p, q));
                if (union.GetArea() == mergedpoly.Sum(p => p.GetArea()))
                {
                    plist[i] = new MergedPiece(union.First().Select(p => new Numerics.Vector2((float)p.X, (float)p.Y)), mergedPieces.SelectMany(p => p.ComponentPieces).Distinct());
                    for (int j = 0; j < plist.Count; j++)
                    {
                        if (localUsed[j]) used[j] = true;
                    }
                }
            }
            return new Puzzle(puzzle.Frames, plist.Where((p, i) => !used[i]).ToList());
        }

        public static IEnumerable<MergedPiece> Merge(Piece a, Piece b, int edgeCount)
        {
            var reversedA = a.Vertexes.Reverse().ToList();
            var vlistB = b.Vertexes.ToList();
            for (int i = 0; i < reversedA.Count; i++)
            {
                for (int j = 0; j < b.Vertexes.Count; j++)
                {
                    int k = 0;
                    Numerics.Vector2 vecA;
                    Numerics.Vector2 vecB;
                    do
                    {
                        vecA = reversedA[(i + k + 1) % reversedA.Count].Location - reversedA[(i + k) % reversedA.Count].Location;
                        vecB = vlistB[(j + k + 1) % vlistB.Count].Location - vlistB[(j + k) % vlistB.Count].Location;
                        k++;
                    } while (vecA == vecB);
                    if (--k < edgeCount) continue; // 指定の数の辺以上連続で一致していなければなし

                    Func<Numerics.Vector2, Numerics.Vector2, MergedPiece> transform = (avec, bvec) =>
                    {
                        var nodeA = reversedA[i].Location;
                        var nodeB = vlistB[j].Location;
                        float angle = (float)VectorHelper.CalcAngle(avec, bvec);
                        if (Math.Abs(angle % (Math.PI / 2)) > 1E-4) return null; // 90度以外はそもそも回転後に頂点が格子状にないので飛ばす
                        bool isBLeft = Numerics.Vector3.Cross(new Numerics.Vector3(avec, 0), new Numerics.Vector3(bvec, 0)).Z > 0;
                        var transformed = b.Offset(-nodeB.X, -nodeB.Y).Rotate(isBLeft ? -angle : angle).Offset(nodeA.X, nodeA.Y);
                        if (((Polygon2)PolygonCalculation.Intersect(transformed.GetPolygon(), a.GetPolygon())).GetArea() > 0)
                            return null;
                        var union = (Polygon2)PolygonCalculation.Union(a.GetPolygon(), transformed.GetPolygon());
                        if (union.Count > 1) return null;
                        return new MergedPiece(union.Single().Select(p => new Numerics.Vector2((float)p.X, (float)p.Y)),
                            new Piece[] { a, transformed });
                    };
                    vecA = reversedA[(i + 1) % reversedA.Count].Location - reversedA[i % reversedA.Count].Location;
                    vecB = vlistB[(j + 1) % vlistB.Count].Location - vlistB[j % vlistB.Count].Location;
                    var res = transform(vecA, vecB);
                    if (res != null) yield return res;
                }
            }
        }
    }
}
