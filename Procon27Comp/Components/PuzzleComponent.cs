using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;
using Procon27Comp.Internal;

namespace Procon27Comp.Components
{
    /// <summary>
    /// 頂点座標とその内角を持つ図形を表します。
    /// </summary>
    public abstract class PuzzleComponent
    {
        /// <summary>
        /// この図形の頂点を取得します。
        /// </summary>
        public LinkedList<Vertex> Vertexes { get; } = new LinkedList<Vertex>();

        /// <summary>
        /// 図形の頂点座標から、<see cref="PuzzleComponent"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="points">図形の頂点座標を持つ<see cref="System.Collections.IEnumerable"/></param>
        public PuzzleComponent(IEnumerable<Vector2> points)
        {
            var list = new LinkedList<Vector2>(points);

            // 時計回りか反時計回りか判定
            // http://www5d.biglobe.ne.jp/~noocyte/Programming/Geometry/PolygonMoment-jp.html#AreaAndDirection
            float s = 0;
            foreach (var node in list.GetNodes())
            {
                Vector2 current = node.Value;
                Vector2 next = node.GetNextValue();
                s += (current.X + next.X) * (next.Y - current.Y);
            }
            bool isclockwise = s > 0; // ディスプレイの座標系(X軸: 右方向, Y軸: 下方向)なので一般の場合と符号が逆

            foreach (var v in isclockwise ? list.GetNodes() : new LinkedList<Vector2>(list.Reverse()).GetNodes())
            {
                double angle = GetAngle(v.GetPreviousValue(), v.Value, v.GetNextValue());
                Vertexes.AddLast(new Vertex(v.Value, angle));
            }
        }

        // 時計回り前提で角度計算
        protected double GetAngle(Vector2 prev, Vector2 middle, Vector2 next)
        {
            var pv = Vector2.Subtract(middle, prev);
            var nv = Vector2.Subtract(next, middle);
            double angle = VectorHelper.CalcAngle(-pv, nv);
            return VectorHelper.IsConcaveVertex(pv, nv) ? 2 * Math.PI - angle : angle;
        }
    }
}
