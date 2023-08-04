namespace NeeView
{
    /// <summary>
    /// キーを保持する
    /// </summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    public interface IHasKey<TKey>
    {
        public TKey Key { get; }
    }
}
