﻿using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,

        /// <summary>
        /// 再帰 s
        /// </summary>
        Recursive = 0x0001,

        /// <summary>
        /// 再帰しない s
        /// </summary>
        NotRecursive = 0x0002,

        /// <summary>
        /// 初期ページを先頭ページにする
        /// </summary>
        FirstPage = 0x0004,

        /// <summary>
        /// 初期ページを最終ページにする
        /// </summary>
        LastPage = 0x0008,

        /// <summary>
        /// 再読み込みフラグ(BookHubで使用)
        /// </summary>
        ReLoad = 0x0010,

        /// <summary>
        /// 履歴の順番を変更しない
        /// </summary>
        KeepHistoryOrder = 0x0020,

        /// <summary>
        /// 可能ならば履歴リストで選択する
        /// </summary>
        SelectHistoryMaybe = 0x0040,

        /// <summary>
        /// 同じフォルダーならば読み込まない
        /// </summary>
        SkipSamePlace = 0x0080,

        /// <summary>
        /// 履歴情報から全て復元
        /// </summary>
        Resume = 0x0100,

        /// <summary>
        /// 再帰、ただし履歴が優先
        /// </summary>
        DefaultRecursive = 0x0200,

        /// <summary>
        /// このアドレスはブックです
        /// </summary>
        IsBook = 0x0400,

        /// <summary>
        /// このアドレスはページです
        /// </summary>
        IsPage = 0x0800,
    };


    public enum BookStartPageType
    {
        Custom,
        FirstPage,
        LastPage,
    }

    public class BookStartPage
    {
        public BookStartPage(BookStartPageType startPageType)
        {
            Debug.Assert(startPageType != BookStartPageType.Custom);
            StartPageType = startPageType;
        }

        public BookStartPage(string pageName)
        {
            Debug.Assert(!string.IsNullOrEmpty(pageName));
            StartPageType = BookStartPageType.Custom;
            PageName = pageName;
        }

        public BookStartPageType StartPageType { get; private set; }
        public string PageName { get; private set; }
    }

    public static class BookLoadOptionHelper
    {
        /// <summary>
        /// 設定を加味した再帰フラグを取得
        /// </summary>
        /// <param name="isRecursived">デフォルト値</param>
        /// <param name="setting">設定</param>
        public static bool CreateIsRecursiveFolder(bool isRecursived, BookLoadOption optios)
        {
            if (optios.HasFlag(BookLoadOption.NotRecursive))
            {
                return false;
            }
            else if (optios.HasFlag(BookLoadOption.Recursive))
            {
                return true;
            }
            else
            {
                return isRecursived;
            }
        }

        public static BookStartPage CreateBookStartPage(string entry, BookLoadOption optios)
        {
            if ((optios & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                return new BookStartPage(BookStartPageType.FirstPage);
            }
            else if ((optios & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                return new BookStartPage(BookStartPageType.LastPage);
            }
            else if (!string.IsNullOrEmpty(entry))
            {
                return new BookStartPage(entry);
            }
            else
            {
                return new BookStartPage(BookStartPageType.FirstPage);
            }
        }
    }

}