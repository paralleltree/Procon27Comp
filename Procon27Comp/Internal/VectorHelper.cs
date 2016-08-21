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

        /// <summary>
        /// 反時計回りの2ベクトルからへこんでいる頂点かどうかを判定します。
        /// </summary>
        /// <param name="prev">対象頂点の前の頂点を起点とするベクトル</param>
        /// <param name="next">対象頂点の次の頂点を終点とするベクトル</param>
        /// <returns>対象頂点が前の頂点から次の頂点へ向かうベクトルの左側にあればTrue、そうでなければFalse。</returns>
        public static bool IsConcaveVertex(Vector2 prev, Vector2 next)
        {
            var composite = Vector2.Add(prev, next);
            return Vector3.Cross(new Vector3(composite, 0), new Vector3(prev, 0)).Z < 0;
        }
    }
}
