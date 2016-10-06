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
                    // 枠描画
                    foreach (var frame in Puzzle.Frames)
                    {
                        frame.GetPolygon().DrawToImage(g, new Pen(Color.Green));
                    }

                    // ピース描画
                    foreach (var sol in Results)
                    {
                        foreach (var piece in sol.Value.History)
                        {
                            piece.DrawToImage(g, new Pen(Color.DarkRed));
                        }
                    }

                });

                canvas.SaveAsPng(path);
            }
        }
    }
}
