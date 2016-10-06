using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;
using Procon27Comp.Components;

namespace Procon27Comp.Internal
{
    public static class PolygonHelper
    {
        public static Vertesaur.Polygon2 GetPolygon(this PuzzleComponent pc)
        {
            return new Vertesaur.Polygon2(pc.Vertexes.Select(p => new Vertesaur.Point2(p.Location.X, p.Location.Y)));
        }

        public static Vertesaur.Polygon2 GetPolygon(this GpcWrapper.Polygon poly)
        {
            return new Vertesaur.Polygon2(poly.Contour.Zip(poly.ContourIsHole, (p, q) => new Vertesaur.Ring2(p.Vertex.Select(r => new Vertesaur.Point2(r.X, r.Y)), q)));
        }

        public static GpcWrapper.Polygon GetGpcPolygon(this Vertesaur.Polygon2 poly)
        {
            var gp = new GpcWrapper.Polygon();
            foreach (var ring in poly)
                gp.AddContour(new GpcWrapper.VertexList(ring.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray()), false);
            return gp;
        }
    }
}
