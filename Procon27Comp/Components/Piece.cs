using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Procon27Comp.Internal;

namespace Procon27Comp.Components
{
    /// <summary>
    /// パズルのピースを表します。
    /// </summary>
    public class Piece : PuzzleComponent
    {
        /// <summary>
        /// 図形の頂点座標から、<see cref="Piece"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="points">図形の頂点座標を持つ<see cref="System.Collections.IEnumerable"/></param>
        public Piece(IEnumerable<Vector2> points) : base(points)
        {
        }

        /// <summary>
        /// ピースを平行移動します。
        /// </summary>
        /// <param name="offsetX">X軸方向の移動距離</param>
        /// <param name="offsetY">Y軸方向の移動距離</param>
        /// <returns>平行移動によってできるピース</returns>
        public Piece Offset(float offsetX, float offsetY)
        {
            return new Piece(Vertexes.Select(p => new Vector2(p.Location.X + offsetX, p.Location.Y + offsetY)));
        }

        /// <summary>
        /// ピースに回転変換を適用します。
        /// </summary>
        /// <param name="src">回転変換を適用するピース</param>
        /// <param name="rad">回転角</param>
        /// <returns>回転変換が適用されたピース</returns>
        public Piece Rotate(float rad)
        {
            return new Piece(Vertexes.Select(p => p.Location.Rotate(rad)));
        }
    }
}
