using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Procon27Comp.Components
{
    /// <summary>
    /// パズルのわくを表します。
    /// </summary>
    public class Frame : PuzzleComponent
    {
        /// <summary>
        /// 図形の頂点座標から、<see cref="Frame"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="points">図形の頂点座標を持つ<see cref="System.Collections.IEnumerable"/></param>
        public Frame(IEnumerable<Vector2> points) : base(points)
        {
        }
    }
}
