using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;
using Procon27Comp.Internal;
using Procon27Comp.Solvers;

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

        public Puzzle ApplyHint(Puzzle hint)
        {
            var frames = Frames.Select(p => p.GetPolygon()).ToList();
            var pieces = Pieces.Select(p => p.GetPolygon()).ToList();
            var hintPieces = hint.Pieces.Select(p => p.GetPolygon()).ToList();
            bool[] used = new bool[Pieces.Count];
            for (int i = 0; i < hintPieces.Count; i++)
            {
                for (int j = 0; j < frames.Count; j++)
                {
                    var diff = PolygonCalculation.Difference(frames[j], hintPieces[i]);
                    if (diff.GetArea() == frames[j].GetArea()) continue;
                    if (diff.Any(p => p.Hole.HasValue && p.Hole.Value)) continue;

                    frames[j] = diff;
                    // ピース同定
                    int index = FindPiece(hint.Pieces[i], Pieces);
                    if (index == -1) throw new InvalidOperationException();
                    used[index] = true;
                }
            }
            return new Puzzle(
                frames.SelectMany(p => p.Select(q => new Frame(q.Select(r => new Vector2((float)r.X, (float)r.Y))))).ToList(),
                Pieces.Where((p, i) => !used[i]).ToList());
        }

        private int FindPiece(Piece piece, List<Piece> pieces)
        {
            var offset = -piece.Vertexes.First().Location;
            piece = piece.Offset(offset.X, offset.Y);
            foreach (var flipPiece in new Piece[] { piece, piece.Flip() })
                foreach (var rotatedPiece in Enumerable.Range(0, 4).Select(p => flipPiece.Rotate(p * 90 * (float)Math.PI / 180)))
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        for (int j = 0; j < pieces[i].Vertexes.Count; j++)
                        {
                            var d = pieces[i].Vertexes.ElementAt(j).Location;
                            var transformed = pieces[i].Offset(-d.X, -d.Y);
                            if (transformed.GetPolygon().SpatiallyEqual(rotatedPiece.GetPolygon())) return i;
                        }
                    }
            return -1;
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
