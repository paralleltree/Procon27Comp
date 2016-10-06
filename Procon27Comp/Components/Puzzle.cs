using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon27Comp.Components
{
    /// <summary>
    /// わくとピースを含むパズルを表します。
    /// </summary>
    public class Puzzle
    {
        /// <summary>
        /// このパズルのわくを取得します。
        /// </summary>
        public List<Frame> Frames { get; }

        /// <summary>
        /// このパズルのピースを取得します。
        /// </summary>
        public List<Piece> Pieces { get; }

        /// <summary>
        /// わくとピースから、<see cref="Puzzle"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="frames">パズルのわく</param>
        /// <param name="pieces">パズルのピース</param>
        public Puzzle(List<Frame> frames, List<Piece> pieces)
        {
            Frames = frames;
            Pieces = pieces;
        }
    }
}
