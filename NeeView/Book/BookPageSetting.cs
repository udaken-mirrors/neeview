using NeeLaboratory.ComponentModel;
using System;

namespace NeeView
{
    /// <summary>
    /// BookPageViewSetting アクセサ。
    /// ProeprtyChangedイベントを追加したもの
    /// </summary>
    public class BookPageSetting : BindableBase
    {
        private readonly BookPageViewSetting _setting;


        public BookPageSetting(BookPageViewSetting setting)
        {
            _setting = setting;
        }

        
        // 設定変更
        public event EventHandler? SettingChanged;

        public IDisposable SubscribeSettingChanged(EventHandler handler)
        {
            SettingChanged += handler;
            return new AnonymousDisposable(() => SettingChanged -= handler);
        }


        public BookPageViewSetting Source => _setting;


        // 横長ページを分割する
        public bool IsSupportedDividePage
        {
            get { return _setting.IsSupportedDividePage; }
            set { if (_setting.IsSupportedDividePage != value) { _setting.IsSupportedDividePage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 最初のページは単独表示
        public bool IsSupportedSingleFirstPage
        {
            get { return _setting.IsSupportedSingleFirstPage; }
            set { if (_setting.IsSupportedSingleFirstPage != value) { _setting.IsSupportedSingleFirstPage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 最後のページは単独表示
        public bool IsSupportedSingleLastPage
        {
            get { return _setting.IsSupportedSingleLastPage; }
            set { if (_setting.IsSupportedSingleLastPage != value) { _setting.IsSupportedSingleLastPage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 横長ページは２ページとみなす
        public bool IsSupportedWidePage
        {
            get { return _setting.IsSupportedWidePage; }
            set { if (_setting.IsSupportedWidePage != value) { _setting.IsSupportedWidePage = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 右開き、左開き
        public PageReadOrder BookReadOrder
        {
            get { return _setting.BookReadOrder; }
            set { if (_setting.BookReadOrder != value) { _setting.BookReadOrder = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

        // 単ページ/見開き
        public PageMode PageMode
        {
            get { return _setting.PageMode; }
            set { if (_setting.PageMode != value) { _setting.PageMode = value; RaisePropertyChanged(); SettingChanged?.Invoke(this, EventArgs.Empty); } }
        }

    }
}
