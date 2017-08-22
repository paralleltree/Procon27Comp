﻿using System;
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
            foreach (var f in forder.OrderBy(p => Math.Abs(p.GetPolygon().GetArea())))
            {
                var first = new State(initf)
                {
                    CurrentFrame = f
                };
                State res = Update(first);
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
            //var rnd = new Random();
            Polygon2 fpolygon = current.CurrentFrame.GetPolygon();

            // わくの面積がほぼ消えたら返す
            // TODO: わくと残りピースの面積比較
            if (Math.Abs(fpolygon.GetArea()) < 10.0)
                return current;

            //bool tried = false;
            foreach (var fn in GetNextFrameVertex(current.CurrentFrame)) // 目標頂点
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
                        // とりあえず+2度まで許容
                        if (pv.Angle - fv.Angle > 2 * Math.PI / 180) continue; // そもそもピースが入らない角度なら飛ばす
                        if (fv.Angle < 10 * Math.PI / 180) continue; // 折れるほど細ければ飛ばす

                        //tried = true;

                        // 対象頂点(pv)を原点に移動して回転
                        // とりあえずわく頂点の次への辺を基準に
                        // 両方反時計回り前提
                        //Numerics.Vector2 fvec = fn.GetNextValue().Location - fn.Value.Location;
                        //Numerics.Vector2 pvec = pn.GetNextValue().Location - pn.Value.Location;
                        //float angle = (float)VectorHelper.CalcAngle(fvec, pvec);
                        //bool ispieceleft = Numerics.Vector3.Cross(new Numerics.Vector3(fvec, 0), new Numerics.Vector3(pvec, 0)).Z > 0;
                        //var transformed = piece.Offset(-pv.X, pv.Y).Rotate(ispieceleft ? -angle : angle);
                        //var ppolygon = new Polygon2(transformed.GetPolygon().Single().Transform(fv.X, fv.Y));

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

                            // Unionとって枠と面積が等しいか
                            // intersetがピースと等しいか
                            double ia = Math.Abs(intersect.GetArea());

                            // bouding-boxではみ出しから最長の辺を縦にして縦横比を見る。極端に小さければ飛ばす
                            var boxrates = ((Polygon2)PolygonCalculation.Difference(ppoly, intersect))
                                .Select(p =>
                                {
                                    var lintersect = new LinkedList<Numerics.Vector2>(p.Select(q => new Numerics.Vector2((float)q.X, (float)q.Y)));
                                    var longvec = lintersect.GetNodes().Select(q => q.GetNextValue() - q.Value)
                                          .OrderByDescending(q => q.LengthSquared())
                                          .First();
                                    float angle = (float)VectorHelper.CalcAngle(longvec, Numerics.Vector2.UnitY);
                                    bool isonleft = Numerics.Vector3.Cross(new Numerics.Vector3(Numerics.Vector2.UnitY, 0), new Numerics.Vector3(longvec, 0)).Z > 0;
                                    var transformed = lintersect.Rotate(isonleft ? -angle : angle).ToList();
                                    var tpoly = new Polygon2(transformed.Select(q => new Point2(q.X, q.Y)));
                                    var box = tpoly.GetMbr();
                                    return box.Width / box.Height;
                                });

                            if (Math.Abs(ia - Math.Abs(ppoly.GetArea())) > 10.0  /* && */)
                            {
                                // 0-1の比の間で判定
                                if (boxrates.Any(p => p > 0.08)) continue;
                            }
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
                            var validframe = merged.Where(p => Math.Abs(p.GetArea()) > 4.0).ToList();

                            // 更新作業
                            //var nexthist = new LinkedList<State>(current.Parent);
                            //nexthist.AddLast(current);


                            var nextstate = new State(current.UnusedFlags & ((1UL << pi) ^ ulong.MaxValue)) // 未使用フラグを折る
                            {
                                Parent = current,
                                Piece = ppoly
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

                                // TODO: とんがりコーンを取り除く？
                                // 重なった角を除く？
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

            //if (tried) return current; // 試してダメならおしまい？
            return null; // そもそも置けそうなのに結局置けなかった
        }

        // ピースの頂点とわくの頂点をもらってわく頂点の左右の辺にくっつける位置でピースを返す
        IEnumerable<Polygon2> GetNextPieces(Piece piece, LinkedListNode<Vertex> piecen, LinkedListNode<Vertex> framen)
        {
            Func<Numerics.Vector2, Numerics.Vector2, Polygon2> transform = (fvec, pvec) =>
            {
                float angle = (float)VectorHelper.CalcAngle(fvec, pvec);
                bool ispieceleft = Numerics.Vector3.Cross(new Numerics.Vector3(fvec, 0), new Numerics.Vector3(pvec, 0)).Z > 0;
                var transformed = piece.Offset(-piecen.Value.X, -piecen.Value.Y).Rotate(ispieceleft ? -angle : angle);
                var res = new Polygon2(transformed.GetPolygon().Single().Transform(framen.Value.X, framen.Value.Y));
                return res;
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
            return dic.OrderBy(p => p.Key.Value.Angle).Select(p => p.Key);
        }
    }

    public class Operation
    {
        public Vector2 Offset { get; set; }
        public double RotateAngle { get; set; }

        /// <summary>
        /// <see cref="Offset"/>で指定された座標だけ平行移動してから
        /// <see cref="RotateAngle"/>で指定した角度の回転を適用した<see cref="Polygon2"/>を返します。
        /// </summary>
        /// <param name="p">適用対象の<see cref="Piece"/></param>
        /// <returns>変換された<see cref="Polygon2"/></returns>
        public Polygon2 ApplyTransform(Piece p)
        {
            throw new NotImplementedException();
        }
    }

    public class PieceState
    {
        public Piece Piece { get; }
        public Operation Operation { get; }

        public PieceState(Piece piece, Operation operation)
        {
            Piece = piece;
            Operation = operation;
        }

        public Polygon2 Calculate()
        {
            throw new NotImplementedException();
        }
    }

    public static class PolygonCalculation
    {
        // 減算対象が内側にあると内側しか返さない
        private static PolygonDifferenceOperation DifferenceOperation = new PolygonDifferenceOperation();
        private static PolygonIntersectionOperation IntersectionOperation = new PolygonIntersectionOperation();
        // 積しか返さない。
        private static PolygonUnionOperation UnionOperation = new PolygonUnionOperation();

        public static IPlanarGeometry Difference(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Difference, b.GetGpcPolygon()).GetPolygon();
            //return DifferenceOperation.Difference(a, b);
        }

        public static IPlanarGeometry Intersect(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Intersection, b.GetGpcPolygon()).GetPolygon();
            //return IntersectionOperation.Intersect(a, b);
        }

        public static IPlanarGeometry Union(Polygon2 a, Polygon2 b)
        {
            return a.GetGpcPolygon().Clip(GpcWrapper.GpcOperation.Union, b.GetGpcPolygon()).GetPolygon();
            //return UnionOperation.Union(a, b);
        }

        public static IPlanarGeometry Invert(Polygon2 a)
        {
            return PolygonInverseOperation.Invert(a);
        }
    }
}
