﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 情報コンテンツ表示用
    /// </summary>
    public class PageMessage
    {
        /// <summary>
        /// アイコン
        /// </summary>
        public FilePageIcon Icon { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string? Message { get; set; }
    }


    /// <summary>
    /// ページコンテンツ基底
    /// </summary>
    public abstract class PageContent : BindableBase, IDisposable
    {
        #region 開発用

        [Conditional("DEBUG")]
        private void InitializeDev()
        {
            Thumbnail.Changed += (s, e) => UpdateDevStatus();
        }

        [Conditional("DEBUG")]
        public void UpdateDevStatus()
        {
            DevStatus = (Thumbnail.IsValid ? "T" : "") + (IsLoaded ? "C" : "");
        }

        private string? _devStatus;
        public string? DevStatus
        {
            get { return _devStatus; }
            set { if (_devStatus != value) { _devStatus = value; RaisePropertyChanged(); } }
        }

        #endregion

        private ArchiveEntry _entry;
        private int _index;
        private PageContentState _state;


        public PageContent(ArchiveEntry entry)
        {
            _entry = entry;

            // 開発用：
            InitializeDev();
        }


        public virtual ArchiveEntry Entry
        {
            get { return _entry; }
            private set { SetProperty(ref _entry, value); }
        }

        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public virtual Size Size => SizeExtensions.Zero;

        /// <summary>
        /// 情報表示用
        /// </summary>
        public PageMessage? PageMessage { get; private set; }

        public Thumbnail Thumbnail { get; } = new Thumbnail();

        /// <summary>
        /// ロード完了
        /// </summary>
        public virtual bool IsLoaded => true;

        /// <summary>
        /// 表示準備完了
        /// </summary>
        public virtual bool IsViewReady => IsLoaded;

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public bool IsContentLocked => _state != PageContentState.None;

        public virtual bool CanResize => false;

        /// <summary>
        /// テンポラリファイル
        /// </summary>
        public FileProxy? FileProxy { get; private set; }


        /// <summary>
        /// 使用メモリサイズ (Picture)
        /// </summary>
        public virtual long GetContentMemorySize() => 0;

        /// <summary>
        /// 使用メモリサイズ (PictureSource)
        /// </summary>
        public virtual long GetPictureSourceMemorySize() => 0;

        /// <summary>
        /// エントリ設定。遅延生成で使用される
        /// </summary>
        public void SetEntry(ArchiveEntry entry)
        {
            Entry = entry;
        }

        /// <summary>
        /// テンポラリファイルの作成
        /// </summary>
        /// <param name="isKeepFileName">エントリ名準拠のテンポラリファイルを作成</param>
        public FileProxy CreateTempFile(bool isKeepFileName)
        {
            ThrowIfDisposed();

            FileProxy = FileProxy ?? Entry.ExtractToTemp(isKeepFileName);
            return FileProxy;
        }

        /// <summary>
        /// メッセージ表示の設定
        /// </summary>
        public void SetPageMessage(PageMessage? message)
        {
            PageMessage = message;
        }

        /// <summary>
        /// メッセージ表示に例外を設定
        /// </summary>
        public void SetPageMessage(Exception ex)
        {
            PageMessage = new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = ex.Message
            };
        }

        public override string? ToString()
        {
            return _entry.EntryLastName ?? base.ToString();
        }

        /// <summary>
        /// クローン作成
        /// </summary>
        /// <returns></returns>
        public PageContent Clone()
        {
            var clone = (PageContent)MemberwiseClone();
            clone.ResetPropertyChanged();
            return clone;
        }

        /// <summary>
        /// ローダー作成
        /// </summary>
        public abstract IContentLoader CreateContentLoader();


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    State = PageContentState.None;
                    FileProxy = null;
                    Thumbnail.Dispose(); //TODO: Thumbnail自体のDisposeの必要性の検証
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }




}
