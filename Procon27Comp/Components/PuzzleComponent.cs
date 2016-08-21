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
            foreach (var v in list.GetNodes())
            {
                var current = v.Value;
                var prev = v.GetPreviousValue();
                var next = v.GetNextValue();

                var pv = Vector2.Subtract(current, prev);
                var nv = Vector2.Subtract(next, current);
                double angle = VectorHelper.CalcAngle(-pv, nv);
                if (VectorHelper.IsConcaveVertex(pv, nv)) angle = 2 * Math.PI - angle;
                Vertexes.AddLast(new Vertex(current, angle));
            }
        }
    }
}
