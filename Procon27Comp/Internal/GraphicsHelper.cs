using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;
using System.Drawing.Imaging;

using Procon27Comp.Components;

namespace Procon27Comp.Internal
{
    public static class GraphicsHelper
    {
        public static void DrawToImage(this Vertesaur.Polygon2 poly, Graphics g, Pen pen)
        {
            foreach (Vertesaur.Ring2 ring in poly)
                g.DrawPolygon(pen, ring.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray());
        }

        public static void WorkWithGraphic(this Image image, Action<Graphics> action)
        {
            using (var g = Graphics.FromImage(image))
            {
                action(g);
            }
        }

        public static void SaveAsPng(this Image image, string path)
        {
            try
            {
                image.Save(path, ImageFormat.Png);
            }
            catch
            {
            }
        }

        public static void DumpToImage(this Puzzle puzzle, string path)
        {
            using (var bmp = new Bitmap(1200, 800))
            {
                WorkWithGraphic(bmp, g =>
                {
                    puzzle.Pieces.Select((p, i) => new { Index = i, Piece = p }).ToList()
                    .ForEach(p =>
                    {
                        p.Piece.GetPolygon().DrawToImage(g, Pens.Blue);
                        var centroid = p.Piece.GetPolygon().GetCentroid();
                        g.DrawString(p.Index.ToString(), new Font("MS Gothic", 10), Brushes.Brown, new PointF((float)centroid.X, (float)centroid.Y));
                    });
                });
                bmp.SaveAsPng(path);
            }
        }
    }
}
