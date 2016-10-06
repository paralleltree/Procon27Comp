using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;
using System.Drawing.Imaging;

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
    }
}
