using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon27Comp.Internal
{
    /// <summary>
    /// 汎用的な拡張メソッドを提供します。
    /// </summary>
    internal static class CoreExtensions
    {
        /// <summary>
        /// ラジアン角を度に変換します。
        /// </summary>
        /// <param name="r">ラジアン角で表されている角度</param>
        /// <returns>度数法で表される変換された角度</returns>
        public static double ToDegrees(this double r) => r * 180 / Math.PI;

        /// <summary>
        /// <see cref="LinkedList{T}"/>の各<see cref="LinkedListNode{T}"/>を列挙します。
        /// </summary>
        public static IEnumerable<LinkedListNode<T>> GetNodes<T>(this LinkedList<T> list)
        {
            for (var current = list.First; current != null; current = current.Next)
            {
                yield return current;
            }
        }

        /// <summary>
        /// 現在の<see cref="LinkedListNode{T}"/>の次のノードの要素を取得します。
        /// 最後のノードであればリストの最初の要素を返します。
        /// </summary>
        /// <returns><param name="node">の次のノードの要素</param></returns>
        /// <exception cref="InvalidOperationException"><paramref name="node"/>の要素数が0のとき</exception>
        public static T GetNextValue<T>(this LinkedListNode<T> node)
        {
            if (node.List.Count < 1) throw new InvalidOperationException("The list does not have any elements.");
            return (node.Next ?? node.List.First).Value;
        }

        /// <summary>
        /// 現在の<see cref="LinkedListNode{T}"/>の前のノードの要素を取得します。
        /// 最初のノードであればリストの最後の要素を返します。
        /// </summary>
        /// <returns><paramref name="node"/>の前のノードの要素</returns>
        /// <exception cref="InvalidOperationException"><paramref name="node"/>の要素数が0のとき</exception>
        public static T GetPreviousValue<T>(this LinkedListNode<T> node)
        {
            if (node.List.Count < 1) throw new InvalidOperationException("The list does not have any elements.");
            return (node.Previous ?? node.List.Last).Value;
        }
    }
}
