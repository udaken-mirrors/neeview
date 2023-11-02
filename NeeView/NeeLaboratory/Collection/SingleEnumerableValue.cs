using System.Collections;
using System.Collections.Generic;

namespace NeeLaboratory.Collection
{
    /// <summary>
    /// オブジェクトを単一の列挙型にする。
    /// </summary>
    /// <remarks>
    /// struct なのでローカル変数として使用する場合はヒープを使用しない、と思う
    /// </remarks>
    public readonly struct SingleEnumerableValue<T> : IEnumerable<T>
    {
        private readonly T _value;

        public SingleEnumerableValue(T value)
        {
            _value = value;
        }

        public readonly IEnumerator<T> GetEnumerator()
        {
            yield return _value;
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
