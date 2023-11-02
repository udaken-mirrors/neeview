using System;

namespace NeeView
{
    /// <summary>
    /// 検索ボックス コンポーネント
    /// </summary>
    public interface ISearchBoxComponent
    {
        /// <summary>
        /// 検索履歴
        /// </summary>
        HistoryStringCollection? History { get; }

        /// <summary>
        /// インクリメンタルサーチ フラグ
        /// </summary>
        bool IsIncrementalSearchEnabled { get; }
        
        /// <summary>
        /// 検索実行
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        void Search(string keyword);
    }


}
