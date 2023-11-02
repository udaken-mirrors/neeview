using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 文字列の履歴コレクション。
    /// 検索コンボボックスの候補リスト等で使用されます。
    /// </summary>
    public class HistoryStringCollection : ObservableCollection<string>
    {
        public HistoryStringCollection()
        {
        }

        public HistoryStringCollection(IEnumerable<string> collection) : base(collection)
        {
        }

        /// <summary>
        /// 履歴最大数
        /// </summary>
        private int MaxCount { get; init; } = 8;


        /// <summary>
        /// 内容全更新
        /// </summary>
        /// <param name="collection"></param>
        public void Replace(IEnumerable<string>? collection)
        {
            using (BlockReentrancy())
            {
                this.Clear();
                if (collection is not null)
                {
                    foreach (var item in collection)
                    {
                        this.Add(item);
                    }
                }
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// 履歴に追加
        /// </summary>
        /// <remarks>
        /// リストの先頭に追加します。空白文字列は同じ文字列は除外します。
        /// </remarks>
        /// <param name="text">追加文字列</param>
        public void Append(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (this.Count <= 0)
            {
                this.Add(text);
            }
            else if (this.First() != text)
            {
                int index = this.IndexOf(text);
                if (index > 0)
                {
                    this.Move(index, 0);
                }
                else
                {
                    this.Insert(0, text);
                }
            }

            while (this.Count > MaxCount)
            {
                this.RemoveAt(this.Count - 1);
            }
        }

    }
}
