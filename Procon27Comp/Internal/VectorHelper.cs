using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;

namespace Procon27Comp.Internal
{
    /// <summary>
    /// ベクトルの演算を行うメソッドを提供します。
    /// </summary>
    internal static class VectorHelper
    {
        /// <summary>
        /// 2ベクトルがなす角度を計算します。
        /// </summary>
        /// <param name="a">1つ目のベクトル</param>
        /// <param name="b">2つ目のベクトル</param>
        /// <returns>与えられた2ベクトルがなす角度</returns>
        /// <remarks>いずれかのベクトルの絶対値が0のとき、NaN。</remarks>
        public static double CalcAngle(Vector2 a, Vector2 b)
        {
            return Math.Acos((Vector2.Dot(a, b) / (a.Length() * b.Length())));
        }
    }
}
