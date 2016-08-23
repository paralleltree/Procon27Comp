using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Procon27Comp.Internal;

namespace Procon27Comp.Components
{
    /// <summary>
    /// パズルのわくを表します。
    /// </summary>
    public class Frame : PuzzleComponent
    {
        /// <summary>
        /// 図形の頂点座標から、<see cref="Frame"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="points">図形の頂点座標を持つ<see cref="System.Collections.IEnumerable"/></param>
        public Frame(IEnumerable<Vector2> points) : base(points)
        {
        }

        /// <summary>
        /// 指定のわくの頂点に対してピースを結合してできる新たなわくを返します。
        /// </summary>
        /// <param name="framev">結合先のわくの頂点</param>
        /// <param name="piece">結合するピース</param>
        /// <param name="piecev">結合するピースの頂点</param>
        /// <returns>結合によって新しくできたわく</returns>
        public Frame PutPiece(LinkedListNode<Vertex> frameNode, LinkedListNode<Vertex> pieceNode)
        {
            var framev = new LinkedList<Vector2>(Vertexes.Select(p => p.Location));

            // 回転量の基準となるベクトル
            //var framepv = Vector2.Subtract(frameNode.Value.Location, frameNode.GetPreviousValue().Location);
            //var piecepv = Vector2.Subtract(pieceNode.Value.Location, pieceNode.GetPreviousValue().Location);
            var framepv = Vector2.Subtract(frameNode.GetNextValue().Location, frameNode.Value.Location);
            var piecepv = Vector2.Subtract(pieceNode.GetNextValue().Location, pieceNode.Value.Location);
            // ピースの結合頂点を原点に引っ張って回転
            var transformedPiece = new LinkedList<Vector2>(pieceNode.List.Select(p => p.Location)
                .Offset(-pieceNode.Value.Location.X, -pieceNode.Value.Location.Y)
                .Rotate((float)VectorHelper.CalcAngle(framepv, -piecepv)));

            var framevNode = framev.Find(frameNode.Value.Location).Next ?? framev.First;
            framev.Remove(framevNode.Previous ?? framev.Last);
            var transformedPieceNode = transformedPiece.Find(new Vector2(0, 0));

            // ピースで埋まるわくの更新
            for (var currentPieceNode = transformedPieceNode;
                currentPieceNode != (transformedPieceNode.Previous ?? transformedPiece.Last);
                currentPieceNode = currentPieceNode.Next ?? transformedPiece.First)
            {
                framev.AddAfter(framevNode, Vector2.Add(framevNode.Value, currentPieceNode.Value));
                framevNode = framevNode.Next;
            }

            return new Frame(framev);
        }
    }
}
