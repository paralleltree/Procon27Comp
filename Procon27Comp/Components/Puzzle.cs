using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;

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

        /// <summary>
        /// QRコードをデコードしたテキストから問題データを作成します。
        /// </summary>
        /// <param name="data">QRコードの内容からなるリスト</param>
        /// <returns>読み込んだ結果の<see cref="Puzzle"/></returns>
        public static Puzzle ReadFromData(IEnumerable<string> list)
        {
            var pieces = new List<Piece>();
            Frame frame = null;

            Func<string, IEnumerable<Vector2>> extractVertexes = s =>
            {
                string[] values = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var vlist = new List<Vector2>();
                for (int j = 1; j < values.Length; j += 2)
                    vlist.Add(new Vector2(float.Parse(values[j]) * 10, float.Parse(values[j + 1]) * 10));
                return vlist;
            };

            foreach (string data in list)
            {
                string[] s = data.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int piecesCount = int.Parse(s[0]);
                for (int i = 1; i <= piecesCount; i++)
                {
                    pieces.Add(new Piece(extractVertexes(s[i])));
                }
                if (s.Length - 1 == piecesCount) continue;

                frame = new Frame(extractVertexes(s[s.Length - 1]));
            }
            return new Puzzle(new List<Frame>(new Frame[] { frame }), pieces);
        }
    }
}
