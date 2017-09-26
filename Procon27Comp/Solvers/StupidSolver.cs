using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        private DateTime startedTime;
        private DateTime finishedTime;
        private Size picSize = new Size(1280, 720);
        public Puzzle Puzzle { get; }
        public List<Solution> Solutions { get; }

        public TimeSpan ExecutionTime { get { return finishedTime - startedTime; } }

        public StupidSolver(Puzzle puzzle)
        {
            Puzzle = puzzle;
            Solutions = new List<Solution>();
        }

        public void Solve()
        {
            if (Solutions.Count > 0) return;
            startedTime = DateTime.Now;
            var ansdic = new Dictionary<Frame, State>(Puzzle.Frames.Count);

            // 初期状態
            ulong initf = State.InitFlags(Puzzle.Pieces.Count);

            // 一番小さいわくから埋める
            var forder = Puzzle.Frames;
            foreach (var f in forder.OrderBy(p => Math.Abs(p.GetPolygon().GetArea())))
            {
                var first = new State(initf)
                {
                    CurrentFrame = f
                };
                State res = Update(first);
                finishedTime = DateTime.Now;
                if (res == null) return; // 失敗
                Console.WriteLine("Found");
                ansdic.Add(f, res);
                initf = res.UnusedFlags; // 未使用ピースを更新
            }
            Console.WriteLine("Done");

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
            if (Math.Abs(fpolygon.GetArea()) < 1.0)
                return current;

            // わくの形状に一致するピースがあればそいつをはめてみる
            foreach (int pi in current.EnumerateUnusedPieceIndices())
            {
                for (var fn = current.CurrentFrame.Vertexes.First; fn != null; fn = fn.Next)
                {
                    var currentf = fn;
                    var currentp = Puzzle.Pieces[pi];

                    for (var pn = currentp.Vertexes.First; pn != null; pn = pn.Next)
                    {
                        var currentpn = pn;

                    }
                }
            }


            foreach (var fn in GetNextFrameVertex(current.CurrentFrame))
            {
                float distl = (fn.GetNextValue().Location - fn.Value.Location).LengthSquared();
                float distr = (fn.GetPreviousValue().Location - fn.Value.Location).LengthSquared();

                Vertex fv = fn.Value;
                // 未使用のピースについて探索
                //foreach (int pi in current.EnumerateUnusedPieceIndices().OrderByDescending(p => Math.Abs(Puzzle.Pieces[p].GetPolygon().GetArea())))
                foreach (int pi in current.EnumerateUnusedPieceIndices().OrderBy(p =>
                {
                    var poly = Puzzle.Pieces[p].GetPolygon();
                    return Math.Abs(poly.GetMagnitude() / poly.GetArea());
                }))
                {
                    piececounter++;
                    Piece piece = Puzzle.Pieces[pi];
                    foreach (var pn in piece.Vertexes.GetNodes())
                    {
                        vertexcounter++;
                        Vertex pv = pn.Value;
                        // とりあえず+2度まで許容
                        if (pv.Angle - fv.Angle > 2 * Math.PI / 180) continue; // そもそもピースが入らない角度なら飛ばす
                        if (fv.Angle < 10 * Math.PI / 180) continue; // 折れるほど細ければ飛ばす

                        // 候補を判定
                        foreach (var nextpiece in GetNextPieces(piece, pn, fn))
                        {
                            tryingcounter++;
                            Console.WriteLine("{0,2}, {1,2}, {2,3}", piececounter, vertexcounter, tryingcounter);

                            // 重複面積
                            var intersect = (Polygon2)PolygonCalculation.Intersect(nextpiece.GetPolygon(), fpolygon);

#if DEBUG
                            using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                            {
                                canvas.WorkWithGraphic(g =>
                                {
                                    foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                    fpolygon.DrawToImage(g, new Pen(Color.Green));
                                    nextpiece.GetPolygon().DrawToImage(g, new Pen(Color.DarkRed));
                                    intersect.DrawToImage(g, new Pen(Color.Blue));
                                    if (intersect != null) intersect.DrawToImage(g, new Pen(Color.Blue));
                                });
                                canvas.SaveAsPng(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "merging.png"));
                            }
#endif

                            // わくからはみ出た面積が大きければ飛ばす
                            double ia = Math.Abs(intersect.GetArea());

                            // わくと重なってたらダメ
                            if (Math.Abs(ia - Math.Abs(nextpiece.GetPolygon().GetArea())) > 1.2) continue;
                            // 更新
                            var merged = (Polygon2)PolygonCalculation.Difference(fpolygon, nextpiece.GetPolygon());

#if DEBUG
                            using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                            {
                                canvas.WorkWithGraphic(g =>
                                {
                                    foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                    fpolygon.DrawToImage(g, new Pen(Color.Green));
                                    if (merged != null) merged.DrawToImage(g, new Pen(Color.DarkOrange));
                                });
                                canvas.SaveAsPng(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "merged.png"));
                            }
#endif

                            // merged内に複数あったら面積小さすぎるものを抜く
                            var validframe = merged.Where(p => Math.Abs(p.GetArea()) > 4.0).ToList();

                            var nextstate = new State(current.UnusedFlags & ((1UL << pi) ^ ulong.MaxValue)) // 未使用フラグを折る
                            {
                                Parent = current,
                                Piece = nextpiece
                            };

                            // 全埋めか残り面積が一定以下になったら返す
                            if (validframe.Count == 0 || Math.Abs(merged.GetArea()) < 1.6)
                            {
                                return nextstate;
                            }

                            // 残った枠ごとに面積順に呼び出し
                            bool succeed = true;
                            foreach (var remaining in validframe.OrderBy(p => Math.Abs(p.GetArea())))
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

#if DEBUG
                                using (var canvas = new Bitmap(picSize.Width, picSize.Height))
                                {
                                    canvas.WorkWithGraphic(g =>
                                    {
                                        foreach (var f in Puzzle.Frames) f.GetPolygon().DrawToImage(g, new Pen(Color.Aquamarine));
                                        fpolygon.DrawToImage(g, new Pen(Color.Green));
                                        nextframe.GetPolygon().DrawToImage(g, new Pen(Color.DarkOrange));
                                    });
                                    canvas.SaveAsPng(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "merged_corrected.png"));
                                }
#endif

                                nextstate.CurrentFrame = nextframe;

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
            //if (nextItem != null && piecen.Value.Angle != framen.Value.Angle) yield return nextItem;
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
