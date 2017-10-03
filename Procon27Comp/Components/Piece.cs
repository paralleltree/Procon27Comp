using System;
using System.Collections.Generic;
using System.Drawing;
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
        public virtual Piece Offset(float offsetX, float offsetY)
        {
            return new Piece(Vertexes.Select(p => new Vector2(p.Location.X + offsetX, p.Location.Y + offsetY)));
        }

        /// <summary>
        /// ピースに回転変換を適用します。
        /// </summary>
        /// <param name="src">回転変換を適用するピース</param>
        /// <param name="rad">回転角</param>
        /// <returns>回転変換が適用されたピース</returns>
        public virtual Piece Rotate(float rad)
        {
            return new Piece(Vertexes.Select(p => p.Location.Rotate(rad)));
        }
    }

    /// <summary>
    /// 複数の<see cref="Piece"/>が結合している1つのピースを表します。
    /// </summary>
    public class MergedPiece : Piece
    {
        public List<Piece> ComponentPieces { get; private set; }

        public MergedPiece(IEnumerable<Vector2> points, IEnumerable<Piece> components) : base(points)
        {
            ComponentPieces = components.SelectMany(p => p is MergedPiece ? (p as MergedPiece).ComponentPieces : new List<Piece>() { p }).ToList();
        }

        public override Piece Offset(float offsetX, float offsetY)
        {
            return new MergedPiece(base.Offset(offsetX, offsetY).Vertexes.Select(p => p.Location), ComponentPieces.Select(p => p.Offset(offsetX, offsetY)));
        }

        public override Piece Rotate(float rad)
        {
            return new MergedPiece(base.Rotate(rad).Vertexes.Select(p => p.Location), ComponentPieces.Select(p => p.Rotate(rad)));
        }

        public override void DrawToImage(Graphics g, Pen pen)
        {
            foreach (Piece item in ComponentPieces)
                item.DrawToImage(g, pen);
        }

        public override string GetTextData()
        {
            var pieces = ComponentPieces.Select(p => string.Format("{0} {1}", p.Vertexes.Count, string.Join(" ", p.Vertexes.Select(q => string.Format("{0} {1}", q.X / 10, q.Y / 10)))));
            return string.Join(":", pieces.ToArray());
        }
    }
}
