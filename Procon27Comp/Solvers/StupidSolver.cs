using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            foreach (var f in forder.OrderBy(p => p.GetPolygon().GetArea()))
            {
                var first = new State(initf)
                {
                    CurrentFrame = f,
                    History = new LinkedList<State>()
                };
                State res = Update(first);
                if (res == null) return; // 失敗
                ansdic.Add(f, res);
                initf = res.UnusedFlags; // 未使用ピースを更新
            }

            // 埋まった
            Solutions.Add(new Solution(Puzzle, ansdic));
        }

        int piececounter = 0;
        int vertexcounter = 0;
        int tryingcounter = 0;
        /// <summary>
        /// 1つの枠について探索を進める
        /// </summary>
        /// <param name="current">現在の状態</param>
        /// <returns>見つかった解</returns>
        public State Update(State current)
        {
            // TODO: 埋めきったかチェックして返す
            Polygon2 fpolygon = current.CurrentFrame.GetPolygon();

            // わくの面積がほぼ消えたら返す
            // TODO: わくと残りピースの面積比較
            if (Math.Abs(fpolygon.GetArea()) < 10.0)
                return current;

            foreach (var fn in GetNextFrameVertex(current.CurrentFrame))
            {
                Vertex fv = fn.Value;
                // 未使用のピースについて探索
                foreach (int pi in current.EnumerateUnusedPieceIndices())
                {
                    piececounter++;
                    Piece piece = Puzzle.Pieces[pi];
                    foreach (var pn in piece.Vertexes.GetNodes())
                    {
                        vertexcounter++;
                        Vertex pv = pn.Value;
                        if (pv.Angle > fv.Angle) continue; // そもそもピースが入らない角度なら飛ばす
                        if (fv.Angle < 10 * Math.PI / 180) continue; // 折れるほど細ければ飛ばす

                        // 候補を判定
                        foreach (var ppoly in GetNextPieces(piece, pn, fn))
                        {
                            tryingcounter++;
                            Console.WriteLine("{0,2}, {1,2}, {2,3}", piececounter, vertexcounter, tryingcounter);

                            // 重複面積
                            var intersect = (Polygon2)PolygonCalculation.Intersect(ppoly, fpolygon);
                            using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                            {
                                canvas.WorkWithGraphic(g =>
                                {
                                    foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                    fpolygon.DrawToImage(g, new Pen(Color.Green));
                                    ppoly.DrawToImage(g, new Pen(Color.DarkRed));
                                    intersect.DrawToImage(g, new Pen(Color.Blue));
                                });
                                canvas.SaveAsPng(@"C:\Users\paltee\Desktop\merging.png");
                            }

                            // わくからはみ出た面積が大きければ飛ばす
                            double ia = Math.Abs(intersect.GetArea());
                            if (Math.Abs(ia - Math.Abs(ppoly.GetArea())) > 1.2) continue;

                            // 更新
                            var merged = (Polygon2)PolygonCalculation.Difference(fpolygon, ppoly);

                            using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                            {
                                canvas.WorkWithGraphic(g =>
                                {
                                    foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                    fpolygon.DrawToImage(g, new Pen(Color.Green));
                                    merged.DrawToImage(g, new Pen(Color.DarkOrange));
                                });
                                canvas.SaveAsPng(@"C:\Users\paltee\Desktop\merged.png");
                            }

                            // merged内に複数あったら面積小さすぎるものを抜く
                            var validframe = merged.Where(p => p.GetArea() > 4.0).ToList();

                            var nexthist = new LinkedList<State>(current.History);
                            nexthist.AddLast(current);

                            var nextstate = new State(current.UnusedFlags & ((1UL << pi) ^ ulong.MaxValue)) // 未使用フラグを折る
                            {
                                History = nexthist,
                                Piece = ppoly
                            };

                            // 全埋めか残り面積が一定以下になったら返す
                            if (validframe.Count == 0 || merged.GetArea() < 1.6) // 1.6??
                            {
                                nexthist.AddLast(nextstate);
                                return nextstate;
                            }

                            // 残った枠ごとに面積順に呼び出し
                            bool succeed = true;
                            foreach (var remaining in validframe.OrderBy(p => p.GetArea()))
                            {
                                // 隣り合う近すぎる頂点を除いてわく作成
                                var validvertex = new LinkedList<Point2>(remaining).GetNodes().Select(p => new
                                {
                                    Point = p,
                                    Distance = (p.GetNextValue() - p.Value).GetMagnitudeSquared()
                                })
                                .Where(p => p.Distance > 2 * 2)
                                .Select(p => p.Point.Value);
                                var nextframe = new Frame(validvertex.Select(p => new Numerics.Vector2((float)p.X, (float)p.Y)));
                                // Nanとか無効な角を除く

                                while (true)
                                {
                                    var correct = nextframe.Vertexes.Where(p => !double.IsNaN(p.Angle) && p.Angle > 0.06).ToList();
                                    if (correct.Count == nextframe.Vertexes.Count) break;
                                    nextframe = new Frame(correct.Select(p => p.Location));
                                }


                                using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                                {
                                    canvas.WorkWithGraphic(g =>
                                    {
                                        foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                        fpolygon.DrawToImage(g, new Pen(Color.Green));
                                        nextframe.GetPolygon().DrawToImage(g, new Pen(Color.DarkOrange));
                                    });
                                    canvas.SaveAsPng(@"C:\Users\paltee\Desktop\merged_corrected.png");
                                }

                                nextstate.CurrentFrame = nextframe;
                                nextstate.Piece = ppoly;

                                var res = Update(nextstate);
                                if (res != null)
                                {
                                    nextstate = res; // 埋めたわくの情報で上書き
                                }
                                else
                                {
                                    succeed = false;
                                    break; // 1つでも埋められないわくがあれば次の候補へ
                                }
                            }
                            if (succeed) return nextstate;
                        }
                    }
                }
                break;
            }

            return null;
        }

        // ピースの頂点とわくの頂点をもらってわく頂点の左右の辺にくっつける位置でピースを返す
        IEnumerable<Polygon2> GetNextPieces(Piece piece, LinkedListNode<Vertex> piecen, LinkedListNode<Vertex> framen)
        {
            Func<Numerics.Vector2, Numerics.Vector2, Polygon2> transform = (fvec, pvec) =>
            {
                float angle = (float)VectorHelper.CalcAngle(fvec, pvec);
                bool ispieceleft = Numerics.Vector3.Cross(new Numerics.Vector3(fvec, 0), new Numerics.Vector3(pvec, 0)).Z > 0;
                var transformed = piece.Offset(-piecen.Value.X, -piecen.Value.Y).Rotate(ispieceleft ? -angle : angle);
                return new Polygon2(transformed.GetPolygon().Single().Transform(framen.Value.X, framen.Value.Y));
            };
            yield return transform(
                framen.GetPreviousValue().Location - framen.Value.Location,
                piecen.GetPreviousValue().Location - piecen.Value.Location);
            yield return transform(
                framen.GetNextValue().Location - framen.Value.Location,
                piecen.GetNextValue().Location - piecen.Value.Location);
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
            return dic.OrderBy(p => p.Value).OrderBy(p => p.Key.Value.Angle).Select(p => p.Key);
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
