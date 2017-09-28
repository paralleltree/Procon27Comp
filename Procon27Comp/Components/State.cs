using System;
using System.Collections.Generic;
using System.Linq;

using Vertesaur;

namespace Procon27Comp.Components
{
    public class State
    {
        public int Score { get; set; }

        // Polygon2はRing2のリストオブジェクト(i.e. Polygon2 = List<Ring2>)
        public List<Frame> CurrentFrame { get; set; }

        /// <summary>
        /// 移動したピース
        /// </summary>
        public Piece Piece { get; set; }

        /// <summary>
        /// 親の状態
        /// </summary>
        public State Parent { get; set; }

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