using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    public class BookMemento : IBookSetting
    {
        // フォルダーの場所
        public string Path { get; set; } = "";

        // 名前
        public string Name => Path.EndsWith(@":\") ? Path : System.IO.Path.GetFileName(Path);

        // 現在ページ
        public string Page { get; set; } = "";

        // 1ページ表示 or 2ページ表示
        public PageMode PageMode { get; set; }

        // 右開き or 左開き
        public PageReadOrder BookReadOrder { get; set; }

        // 横長ページ分割 (1ページモード)
        public bool IsSupportedDividePage { get; set; }

        // 最初のページを単独表示 
        public bool IsSupportedSingleFirstPage { get; set; }

        // 最後のページを単独表示
        public bool IsSupportedSingleLastPage { get; set; }

        // 横長ページを2ページ分とみなす(2ページモード)
        public bool IsSupportedWidePage { get; set; } = true;

        // フォルダーの再帰
        public bool IsRecursiveFolder { get; set; }

        // ページ並び順
        public PageSortMode SortMode { get; set; }


        /// <summary>
        /// 複製
        /// </summary>
        public BookMemento Clone()
        {
            return (BookMemento)this.MemberwiseClone();
        }

        // 保存用バリデート
        // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページ等は不要
        public void ValidateForDefault()
        {
            Path = "";
            Page = "";
        }

        // バリデートされたクローン
        public BookMemento ValidatedClone()
        {
            var clone = this.Clone();
            clone.ValidateForDefault();
            return clone;
        }

        // 値の等価判定
        public bool IsEquals(BookMemento? other)
        {
            return other is not null &&
                   Path == other.Path &&
                   Name == other.Name &&
                   Page == other.Page &&
                   PageMode == other.PageMode &&
                   BookReadOrder == other.BookReadOrder &&
                   IsSupportedDividePage == other.IsSupportedDividePage &&
                   IsSupportedSingleFirstPage == other.IsSupportedSingleFirstPage &&
                   IsSupportedSingleLastPage == other.IsSupportedSingleLastPage &&
                   IsSupportedWidePage == other.IsSupportedWidePage &&
                   IsRecursiveFolder == other.IsRecursiveFolder &&
                   SortMode == other.SortMode;
        }
    }
}

