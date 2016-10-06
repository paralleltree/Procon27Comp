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
            float f = Vector2.Dot(a, b) / (a.Length() * b.Length());
            return Math.Acos(f > 1 || f < -1 ? Math.Round(f) : f); // 1を超える場合がある
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

        /// <summary>
        /// 原点を中心にベクトルに回転変換を適用します。
        /// </summary>
        /// <param name="v">変換を適用するベクトル</param>
        /// <param name="rad">回転角</param>
        /// <returns>回転変換が適用されたベクトル</returns>
        public static Vector2 Rotate(this Vector2 v, float rad)
        {
            return Vector2.Transform(v, Matrix3x2.CreateRotation(rad));
        }

        /// <summary>
        /// ベクトルのリストを平行移動します。
        /// </summary>
        /// <param name="offsetX">X軸方向の移動距離</param>
        /// <param name="offsetY">Y軸方向の移動距離</param>
        /// <returns>平行移動を適用したベクトル</returns>
        public static IEnumerable<Vector2> Offset(this IEnumerable<Vector2> list, float offsetX, float offsetY)
        {
            return list.Select(p => new Vector2(p.X + offsetX, p.Y + offsetY));
        }

        /// <summary>
        /// ベクトルのリストに回転変換を適用します。
        /// </summary>
        /// <param name="rad">回転角</param>
        /// <returns>回転変換を適用したベクトル</returns>
        public static IEnumerable<Vector2> Rotate(this IEnumerable<Vector2> list, float rad)
        {
            return list.Select(p => p.Rotate(rad));
        }
    }
}
