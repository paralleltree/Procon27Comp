using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ConcurrentPriorityQueue;
using Vertesaur;
using Vertesaur.PolygonOperation;
using Numerics = System.Numerics;
using Procon27Comp.Components;
using Procon27Comp.Internal;

using System.Drawing;

namespace Procon27Comp.Solvers
{
    /// <summary>
    /// なんちゃってソルバ
    /// </summary>
    public class StupidSolver
    {
        private Size picSize = new Size(1280, 720);
        public Puzzle Puzzle { get; }
        public List<Solution> Solutions { get; }

        public StupidSolver(Puzzle puzzle)
        {
            Puzzle = puzzle;
            Solutions = new List<Solution>();
        }

        public void Solve()
        {
            if (Solutions.Count > 0) return;
            var ansdic = new Dictionary<Frame, State>(Puzzle.Frames.Count);

            // 初期状態
            ulong initf = State.InitFlags(Puzzle.Pieces.Count);

            // 一番小さいわくから埋める
            var forder = Puzzle.Frames;
            foreach (var f in forder.OrderBy(p => Math.Abs(p.GetPolygon().GetArea())))
            {
                var queue = new ConcurrentPriorityQueue<State, int>();
                var first = new State(initf)
                {
                    CurrentFrame = new List<Frame>() { f }
                };
                queue.Enqueue(first, 0);

                while (queue.Count > 0)
                {
                    var state = queue.Dequeue();
                    foreach (var frame in state.CurrentFrame)
                    {
                        var polygonf = frame.GetPolygon();

                        // わくの各頂点に対して全ピースはめちゃう
                        foreach (var nodef in frame.Vertexes.GetNodes())
                        {
                            foreach (int pi in state.EnumerateUnusedPieceIndices())
                            {
                                var piece = Puzzle.Pieces[pi];
                                foreach (var nodep in piece.Vertexes.GetNodes())
                                {
                                    foreach (var nextp in GetNextPieces(piece, nodep, nodef))
                                    {
                                        var polygonp = nextp.GetPolygon();
                                        var intersect = (Polygon2)PolygonCalculation.Intersect(polygonf, polygonp);
                                        if (Math.Abs(polygonp.GetArea()) - Math.Abs(intersect.GetArea()) > 1e-1) continue;

                                        var merged = (Polygon2)PolygonCalculation.Difference(polygonf, polygonp);

                                        // 評価値算出
                                        int score = 0;
                                        var reversedPiece = polygonp.Single().Select(p => new Numerics.Vector2((float)p.X, (float)p.Y)).ToList();
                                        var frameList = frame.Vertexes.ToList();
                                        for (int i = 0; i < reversedPiece.Count; i++)
                                        {
                                            for (int j = 0; j < frameList.Count; j++)
                                            {
                                                int k = 0;
                                                Numerics.Vector2 pv, nv;
                                                do
                                                {
                                                    pv = reversedPiece[(i + k) % reversedPiece.Count];
                                                    nv = frameList[(j + k) % frameList.Count].Location;
                                                    k++;
                                                } while (pv == nv);
                                                if (--k > score) score = k;
                                            }
                                        }

                                        // 全埋め or 面積なくなったら返す
                                        var nextState = new State(state.UnusedFlags & ((1UL << pi) ^ ulong.MaxValue))
                                        {
                                            Parent = state,
                                            Piece = nextp,
                                            Score = state.Score + score,
                                            CurrentFrame = merged.Select(p => new Frame(p.Select(q => new Numerics.Vector2((float)q.X, (float)q.Y)))).ToList()
                                        };

                                        if (merged.Count == 0)
                                        {
                                            ansdic.Add(f, nextState);
                                            initf = nextState.UnusedFlags; // 未使用ピースを更新
                                            goto nextFrame;
                                        }

                                        queue.Enqueue(nextState, nextState.Score);
                                    }
                                }
                            }
                        }
                    }
                }
                nextFrame:;
            }

            // 埋まった
            Solutions.Add(new Solution(Puzzle, ansdic));
        }

        // ピースの頂点とわくの頂点をもらってわく頂点の左右の辺にくっつける位置でピースを返す
        IEnumerable<Piece> GetNextPieces(Piece piece, LinkedListNode<Vertex> piecen, LinkedListNode<Vertex> framen)
        {
            Func<Numerics.Vector2, Numerics.Vector2, Piece> transform = (fvec, pvec) =>
            {
                float angle = (float)VectorHelper.CalcAngle(fvec, pvec);
                if (Math.Abs(angle % (Math.PI / 2)) > 1E-4) return null; // 90度以外はそもそも回転後に頂点が格子状にないので飛ばす
                bool ispieceleft = Numerics.Vector3.Cross(new Numerics.Vector3(fvec, 0), new Numerics.Vector3(pvec, 0)).Z > 0;
                var transformed = piece.Offset(-piecen.Value.X, -piecen.Value.Y).Rotate(ispieceleft ? -angle : angle).Offset(framen.Value.X, framen.Value.Y);
                return transformed;
            };
            var prevItem = transform(
                framen.GetPreviousValue().Location - framen.Value.Location,
                piecen.GetPreviousValue().Location - piecen.Value.Location);
            var nextItem = transform(
                framen.GetNextValue().Location - framen.Value.Location,
                piecen.GetNextValue().Location - piecen.Value.Location);
            if (prevItem != null) yield return prevItem;
            if (nextItem != null) yield return nextItem;
        }

        // 次のわくのターゲット頂点を返す(わく頂点の評価関数)
        IEnumerable<LinkedListNode<Vertex>> GetNextFrameVertex(Frame frame)
        {
            var dic = new Dictionary<LinkedListNode<Vertex>, float>(frame.Vertexes.Count);
            foreach (var v in frame.Vertexes.GetNodes())
            {
                float dist = (v.GetPreviousValue().Location - v.Value.Location).LengthSquared() +
                    (v.GetNextValue().Location - v.Value.Location).LengthSquared();
                dic.Add(v, dist);
            }
            return dic.OrderBy(p => p.Key.Value.Angle).ThenBy(p => p.Value).Select(p => p.Key);
        }
    }

    public static class PolygonCalculation
    {
        public static IPlanarGeometry Difference(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Difference, b.GetGpcPolygon()).GetPolygon();
        }

        public static IPlanarGeometry Intersect(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Intersection, b.GetGpcPolygon()).GetPolygon();
        }

        public static IPlanarGeometry Union(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Union, b.GetGpcPolygon()).GetPolygon();
        }
    }
}
