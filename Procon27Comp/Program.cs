using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using Procon27Comp.Internal;
using Procon27Comp.Components;

namespace Procon27Comp
{
    class Program
    {
        static void Main(string[] args)
        {
            new Vector2(10, 0).Rotate((float)Math.PI / 2);
            VectorHelper.CalcAngle(new Vector2(0, 0), new Vector2(0, 10));
            VectorHelper.IsConcaveVertex(new Vector2(5, 5), new Vector2(10, 11));
            var piece = new Piece(new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(2, 5),
                new Vector2(0, 10),
                new Vector2(10, 10)
            });

            foreach (var v in piece.Vertexes)
            {
                //Console.WriteLine("X: {0}, Y: {1}, Angle: {2}°", v.Location.X, v.Location.Y, v.Angle.ToDegrees());
            }

            var frame = new Frame(new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 12),
                new Vector2(5, 10),
                new Vector2(15, 14),
                new Vector2(15, 0)
            });

            var updated = frame.PutPiece(frame.Vertexes.First.Next, piece.Vertexes.First);
            foreach (var v in updated.Vertexes)
            {
                Console.WriteLine("X: {0}, Y: {1}, Angle: {2}°", v.Location.X, v.Location.Y, v.Angle.ToDegrees());
            }
        }
    }
}
