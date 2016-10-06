using System;
using System.Collections.Generic;
using System.Linq;

using Vertesaur;

namespace Procon27Comp.Components
{
    public class State
    {
        // Polygon2はRing2のリストオブジェクト(i.e. Polygon2 = List<Ring2>)
        public Frame CurrentFrame { get; set; }

        // 移動済みピースのリスト
        public LinkedList<Polygon2> History { get; set; }
        public ulong UnusedFlags { get; set; } // 使われてなかったらビットを立てる

        public State(ulong uflags)
        {
            UnusedFlags = uflags;
        }


        public IEnumerable<int> EnumerateUnusedPieceIndices()
        {
            for (int i = 0; i < 63; i++)
            {
                if (((1UL << i) & UnusedFlags) != 0) yield return i;
            }
        }

        public static ulong InitFlags(int pcount)
        {
            ulong f = 0;
            for (int i = 0; i < pcount; i++)
                f |= 1UL << i;
            return f;
        }
    }
}