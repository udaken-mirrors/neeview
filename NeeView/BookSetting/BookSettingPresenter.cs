using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public partial class BookSettingPresenter : BindableBase
    {
        static BookSettingPresenter() => Current = new BookSettingPresenter();
        public static BookSettingPresenter Current { get; }


        private BookSettingPresenter()
        {
            SettingChanged += (s, e) => RaisePropertyChanged(nameof(LatestSetting));
        }


        // 設定の変更通知
        [Subscribable]
        public event EventHandler<BookSettingEventArgs>? SettingChanged;


        public BookSettingConfig DefaultSetting => Config.Current.BookSettingDefault;

        public BookSettingConfig LatestSetting => Config.Current.BookSetting;

        public BookSettingPolicyConfig Generater => Config.Current.BookSettingPolicy;

        public bool IsLocked { get; set; }


        public void SetLatestSetting(BookSettingConfig setting)
        {
            if (setting == null) return;
            if (!LatestSetting.Equals(setting))
            {
                setting.CopyTo(LatestSetting);
                LatestSetting.Page = "";
                SettingChanged?.Invoke(this, BookSettingEventArgs.Empty);
            }
        }

        // 新しい本の設定
        public BookSettingConfig GetSetting(BookSettingConfig? restore, bool isDefaultRecursive)
        {
            // TODO: isRecursived
            return Generater.Mix(DefaultSetting, LatestSetting, restore, isDefaultRecursive);
        }

        #region BookSetting Operation

        // 単ページ/見開き表示設定の可否
        public bool CanPageModeSubSetting(PageMode mode)
        {
            return !IsLocked && LatestSetting.PageMode == mode;
        }

        // 単ページ/見開き表示設定
        public void SetPageMode(PageMode mode)
        {
            if (IsLocked) return;
            if (LatestSetting.PageMode != mode)
            {
                LatestSetting.PageMode = mode;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.PageMode));
            }
        }

        public void TogglePageMode(int direction, bool isLoop)
        {
            SetPageMode(LatestSetting.PageMode.GetToggle(direction, isLoop));
        }

        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            if (IsLocked) return;
            if (LatestSetting.BookReadOrder != order)
            {
                LatestSetting.BookReadOrder = order;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.BookReadOrder));
            }
        }

        public void ToggleBookReadOrder()
        {
            SetBookReadOrder(LatestSetting.BookReadOrder.GetToggle());
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleFirstPage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedSingleFirstPage != value)
            {
                LatestSetting.IsSupportedSingleFirstPage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleFirstPage));
            }
        }

        public void ToggleIsSupportedSingleFirstPage()
        {
            SetIsSupportedSingleFirstPage(!LatestSetting.IsSupportedSingleFirstPage);
        }

        // 最終ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleLastPage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedSingleLastPage != value)
            {
                LatestSetting.IsSupportedSingleLastPage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleLastPage));
            }
        }

        public void ToggleIsSupportedSingleLastPage()
        {
            SetIsSupportedSingleLastPage(!LatestSetting.IsSupportedSingleLastPage);
        }

        // 横長ページの分割ON/OFF
        public void SetIsSupportedDividePage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedDividePage != value)
            {
                LatestSetting.IsSupportedDividePage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedDividePage));
            }
        }

        public void ToggleIsSupportedDividePage()
        {
            SetIsSupportedDividePage(!LatestSetting.IsSupportedDividePage);
        }

        // 横長ページの見開き判定ON/OFF
        public void SetIsSupportedWidePage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedWidePage != value)
            {
                LatestSetting.IsSupportedWidePage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedWidePage));
            }
        }

        public void ToggleIsSupportedWidePage()
        {
            SetIsSupportedWidePage(!LatestSetting.IsSupportedWidePage);
        }

        // フォルダー再帰読み込みON/OFF
        public void SetIsRecursiveFolder(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsRecursiveFolder != value)
            {
                LatestSetting.IsRecursiveFolder = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsRecursiveFolder));
            }
        }

        public void ToggleIsRecursiveFolder()
        {
            SetIsRecursiveFolder(!LatestSetting.IsRecursiveFolder);
        }

        // ページ並び設定切り替え
        public void ToggleSortMode(PageSortModeClass pageSortModeClass)
        {
            if (IsLocked) return;
            LatestSetting.SortMode = pageSortModeClass.GetTogglePageSortMode(LatestSetting.SortMode);
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.SortMode));
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            if (IsLocked) return;
            LatestSetting.SortMode = mode;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.SortMode));
        }

        // 既定設定を適用
        public void SetDefaultPageSetting()
        {
            if (IsLocked) return;
            SetLatestSetting(DefaultSetting);
        }

        #endregion
    }
}
