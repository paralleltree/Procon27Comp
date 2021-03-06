﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ConcurrentPriorityQueue;
using System.Numerics;
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
        private List<Piece> FlippedPieces { get; }

        private Solution _solution = null;
        public Solution Solution { get { return _solution; } }

        public int InitialBeamWidth { get; set; } = 1;

        private System.Threading.CancellationToken CancellationToken { get; }

        private double minAngle = 0;
        private double minLengthSquared = 0;
        private double minArea = 0;

        public StupidSolver(Puzzle puzzle)
        {
            Puzzle = puzzle;
            FlippedPieces = puzzle.Pieces.Select(p => p.Flip()).ToList();
        }

        public StupidSolver(Puzzle puzzle, System.Threading.CancellationToken cancellationToken) : this(puzzle)
        {
            CancellationToken = cancellationToken;
        }

        public void Solve()
        {
            if (Solution != null) return;
            var ansdic = new Dictionary<Frame, State>(Puzzle.Frames.Count);

            minLengthSquared = Puzzle.Pieces.SelectMany(p => p.Vertexes.GetNodes().Select(q => (q.GetNextValue().Location - q.Value.Location).LengthSquared())).Min();
            minAngle = Puzzle.Pieces.SelectMany(p => p.Vertexes).Select(p => p.Angle).Min();
            minArea = Puzzle.Pieces.Min(p => p.GetPolygon().GetArea());

            // 初期状態
            ulong initf = State.InitFlags(Puzzle.Pieces.Count);

            // 一番小さいわくから埋める
            var forder = Puzzle.Frames;
            foreach (var f in forder.OrderBy(p => Math.Abs(p.GetPolygon().GetArea())))
            {
                var queues = new ConcurrentPriorityQueue<State, int>[Puzzle.Pieces.Count + 1];
                for (int i = 0; i < queues.Length; i++) queues[i] = new ConcurrentPriorityQueue<State, int>();

                var first = new State(initf)
                {
                    CurrentFrame = new List<Frame>() { f }
                };
                queues[0].Enqueue(first, 0);

                int width = InitialBeamWidth;
                while (true)
                {
                    for (int t = 0; t <= Puzzle.Pieces.Count; t++)
                    {
                        for (int k = 0; k < width; k++)
                        {
                            CancellationToken.ThrowIfCancellationRequested();

#if DEBUG
                            Console.WriteLine(queues[t].Count);
#endif
                            if (queues[t].Count == 0) break;
                            var state = queues[t].Dequeue();

#if DEBUG
                            using (var bmp = new Bitmap(1280, 720))
                            {
                                bmp.WorkWithGraphic(g =>
                                {
                                    for (int i = 0; i < state.CurrentFrame.Count; i++)
                                    {
                                        g.DrawPolygon(Pens.Blue, state.CurrentFrame[i].Vertexes.Select(p => new PointF(p.X, p.Y)).ToArray());
                                    }
                                    if (state.Parent != null) g.DrawPolygon(Pens.DarkRed, state.Piece.Vertexes.Select(p => new PointF(p.X, p.Y)).ToArray());
                                });
                                bmp.SaveAsPng(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "prog.png"));
                            }
#endif

                            if (state.CurrentFrame.Count == 0)
                            {
                                ansdic.Add(f, state);
                                initf = state.UnusedFlags; // 未使用ピースを更新
                                goto nextLoop;
                            }

                            foreach (var nextState in ExpandNodes(state))
                                queues[t + 1].Enqueue(nextState, nextState.Score);
                        }
                    }
                    width += 4;
                }
                nextLoop:;
            }

            // 埋まった
            _solution = new Solution(Puzzle, ansdic);
        }

        private IEnumerable<State> ExpandNodes(State state)
        {
            for (int fi = 0; fi < state.CurrentFrame.Count; fi++)
            {
                var frame = state.CurrentFrame[fi];
                var polygonf = frame.GetPolygon();

                // わくの各頂点に対して全ピースはめちゃう
                foreach (var nodef in frame.Vertexes.GetNodes())
                {
                    foreach (int pi in state.EnumerateUnusedPieceIndices())
                    {
                        foreach (var piece in new Piece[] { Puzzle.Pieces[pi], FlippedPieces[pi] })
                        {
                            foreach (var nodep in piece.Vertexes.GetNodes())
                            {
                                foreach (var nextp in GetNextPieces(piece, nodep, nodef))
                                {
                                    var polygonp = nextp.GetPolygon();
                                    var intersect = PolygonCalculation.Intersect(polygonf, polygonp);
                                    if (Math.Abs(polygonp.GetArea()) - Math.Abs(intersect.GetArea()) > 1e-1) continue;

                                    var merged = PolygonCalculation.Difference(polygonf, polygonp);

                                    // 評価値算出
                                    int score = 0;
                                    var pieceList = polygonp.Single().Select(p => new Vector2((float)p.X, (float)p.Y)).ToList();
                                    var frameList = frame.Vertexes.ToList();
                                    for (int i = 0; i < pieceList.Count; i++)
                                    {
                                        for (int j = 0; j < frameList.Count; j++)
                                        {
                                            int k = 0;
                                            Vector2 pv, nv;
                                            do
                                            {
                                                pv = pieceList[(i + k) % pieceList.Count];
                                                nv = frameList[(j + k) % frameList.Count].Location;
                                                k++;
                                            } while (pv == nv && k <= Math.Min(pieceList.Count, frameList.Count));
                                            if (--k > score) score = k;
                                        }
                                    }

                                    var nextFrames = merged.Select(p => new Frame(p.Select(q => new Vector2((float)q.X, (float)q.Y))));

                                    if (nextFrames.Any(p => p.Vertexes.Any(q => q.Angle < minAngle))) continue;
                                    if (nextFrames.Any(p => p.Vertexes.GetNodes().Any(q => (q.GetNextValue().Location - q.Value.Location).LengthSquared() < minLengthSquared))) continue;
                                    if (nextFrames.Any(p => p.GetPolygon().GetArea() < minArea)) continue;

                                    var newFrames = new List<Frame>(state.CurrentFrame);
                                    newFrames.RemoveAt(fi);
                                    newFrames.AddRange(nextFrames);
                                    var nextState = new State(state.UnusedFlags & ((1UL << pi) ^ ulong.MaxValue))
                                    {
                                        Parent = state,
                                        Piece = nextp,
                                        Score = state.Score + score,
                                        CurrentFrame = newFrames
                                    };

                                    yield return nextState;

                                    if (merged.Count == 0)
                                    {
                                        goto nextFrame;
                                    }
                                }
                            }
                        }
                    }
                }
                nextFrame:;
            }
        }

        // ピースの頂点とわくの頂点をもらってわく頂点の左右の辺にくっつける位置でピースを返す
        IEnumerable<Piece> GetNextPieces(Piece piece, LinkedListNode<Vertex> piecen, LinkedListNode<Vertex> framen)
        {
            Func<Vector2, Vector2, Piece> transform = (fvec, pvec) =>
            {
                float angle = (float)VectorHelper.CalcAngle(fvec, pvec);
                if (Math.Abs(angle % (Math.PI / 2)) > 1E-4) return null; // 90度以外はそもそも回転後に頂点が格子状にないので飛ばす
                bool ispieceleft = Vector3.Cross(new Vector3(fvec, 0), new Vector3(pvec, 0)).Z > 0;
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
            if (nextItem != null && piecen.Value.Angle != framen.Value.Angle) yield return nextItem;
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
        public static Vertesaur.Polygon2 Difference(Vertesaur.Polygon2 a, Vertesaur.Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Difference, b.GetGpcPolygon()).GetPolygon();
        }

        public static Vertesaur.Polygon2 Intersect(Vertesaur.Polygon2 a, Vertesaur.Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Intersection, b.GetGpcPolygon()).GetPolygon();
        }

        public static Vertesaur.Polygon2 Union(Vertesaur.Polygon2 a, Vertesaur.Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Union, b.GetGpcPolygon()).GetPolygon();
        }
    }
}
