using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;
using Procon27Comp.Components;
using Procon27Comp.Internal;

namespace Procon27Comp
{
    /// <summary>
    /// パズルの解を表します。
    /// </summary>
    public class Solution
    {
        public Puzzle Puzzle { get; }
        public Dictionary<Frame, State> Results { get; }

        public Solution(Puzzle puzzle, Dictionary<Frame, State> result)
        {
            Puzzle = puzzle;
            Results = result;
        }

        /// <summary>
        /// 解を指定のパスへ画像として出力します。
        /// </summary>
        /// <param name="path"></param>
        public void DumpToImage(string path)
        {
            using (var canvas = new Bitmap(1280, 720))
            {
                canvas.WorkWithGraphic(g =>
                {
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, 1280, 720));
                    // 枠描画
                    foreach (var frame in Puzzle.Frames)
                    {
                        frame.GetPolygon().DrawToImage(g, new Pen(Color.Green));
                    }

                    // ピース描画
                    foreach (var sol in Results)
                    {
                        State state = sol.Value;
                        if (state == null) continue;
                        while (state.Parent != null)
                        {
                            state.Piece.DrawToImage(g, new Pen(Color.DarkRed));
                            state = state.Parent;
                        }
                    }

                });

                canvas.SaveAsPng(path);
            }
        }

        public void DumpToText(string path)
        {
            var pieces = Results.SelectMany(p =>
            {
                State state = p.Value;
                if (state == null) return Enumerable.Empty<string>();
                var list = new List<string>();
                while (state.Parent != null)
                {
                    list.Add(state.Piece.GetTextData());
                    state = state.Parent;
                }
                return list;
            }).ToArray();

            string piecesData = string.Join(":", pieces);
            int piecesCount = piecesData.Length - piecesData.Replace(":", "").Length + 1;

            string result = (piecesCount) + ":" + piecesData + ":" + Puzzle.Frames.Single().GetTextData();
            System.IO.File.WriteAllText(path, result);
        }
    }
}
