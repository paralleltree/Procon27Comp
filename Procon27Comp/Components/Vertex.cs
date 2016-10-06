using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;

namespace Procon27Comp.Components
{
    /// <summary>
    /// 座標と内角を持った図形の頂点を表します。
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// 頂点の座標を取得します。
        /// </summary>
        public Vector2 Location { get; }

        /// <summary>
        /// 頂点が持つ内角を取得します。
        /// </summary>
        public double Angle { get; }

        /// <summary>
        /// 頂点のX座標を取得します。
        /// </summary>
        public float X
        {
            get { return Location.X; }
        }

        /// <summary>
        /// 頂点のY座標を取得します。
        /// </summary>
        public float Y
        {
            get { return Location.Y; }
        }


        /// <summary>
        /// 頂点の座標と内角から、<see cref="Vertex"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="loc">頂点の座標</param>
        /// <param name="angle">頂点の内角</param>
        public Vertex(Vector2 loc, double angle)
        {
            Location = loc;
            Angle = angle;
        }
    }
}
