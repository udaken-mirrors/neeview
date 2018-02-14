﻿using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Configure
{
    public class SettingPageVisual : SettingPage
    {
        public SettingPageVisual() : base("表示")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageVisualGeneral(),
                new SettingPageVisualWindowTitile(),
                new SettingPageVisualNotify(),
                new SettingPageVisualSlider(),
                new SettingPageVisualThumbnail(),
                new SettingPageVisualSlideshow(),
            };
        }
    }

    public class SettingPageVisualGeneral : SettingPage
    {
        public SettingPageVisualGeneral() : base("表示全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("背景",
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvasBrush.Current, nameof(ContentCanvasBrush.CustomBackground)),
                        new BackgroundSettingControl(ContentCanvasBrush.Current.CustomBackground))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.AutoHideDelayTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(WindowShape.Current, nameof(WindowShape.WindowChromeFrame)))),
            };
        }
    }

    public class SettingPageVisualNotify : SettingPage
    {
        public SettingPageVisualNotify() : base("通知")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("通知表示",
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NoticeShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.CommandShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.GestureShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NowLoadingShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.ViewTransformShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsOriginalScaleShowMessage)))),
            };
        }
    }

    public class SettingPageVisualWindowTitile : SettingPage
    {
        readonly static string _windowTitleFormatTips = $@"フォーマットの説明

$Book  開いているブック名
$Page  現在ページ番号
$PageMax  最大ページ番号
$ViewScale  ビュー操作による表示倍率(%)
$FullName[LR]  パスを含むファイル名
$Name[LR]  ファイル名
$Size[LR]  ファイルサイズ(ex. 100×100)
$SizeEx[LR]  ファイルサイズ + ピクセルビット数(ex. 100×100×24)
$Scale[LR]  画像の表示倍率(%)

""◯◯◯[LR]"" は、1ページ用、2ページ用で変数名が変わることを示します。
例えば $Name は1ページ用、 $NameL は２ページ左用、 $NameR は2ページ右用になります。
$Name は2ページ表示時には主となるページ(ページ番号の小さい方)になります。";

        public SettingPageVisualWindowTitile() : base("ウィンドウタイトル")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("表示",
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat1))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat2))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleWindowTitle)))),

                new SettingItemNote(_windowTitleFormatTips),
            };
        }
    }

    public class SettingPageVisualSlider : SettingPage
    {
        public SettingPageVisualSlider() : base("スライダー")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("スライダー",
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderDirection))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderIndexLayout)))),
            };
        }
    }

    public class SettingPageVisualThumbnail : SettingPage
    {
        public SettingPageVisualThumbnail() : base("サムネイル")
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection("サムネイルリスト",
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.ThumbnailSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.IsSliderLinkedThumbnailList))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailNumber))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailPlate))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsSelectedCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsManipulationBoundaryFeedbackEnabled)))),

                new SettingItemSection("パネルでの表示",
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.ThumbnailWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.BannerWidth)))),

                new SettingItemSection("キャッシュ",
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.IsCacheEnabled))),
                    new SettingItemButton("サムネイルキャッシュを削除する", "削除する",  RemoveCache)),

               new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Format))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Quality))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.PageCapacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.BookCapacity)))),
            };
        }

        #region Commands

        /// <summary>
        /// RemoveCache command.
        /// </summary>
        private RelayCommand<UIElement> _RemoveCache;
        public RelayCommand<UIElement> RemoveCache
        {
            get { return _RemoveCache = _RemoveCache ?? new RelayCommand<UIElement>(RemoveCache_Executed); }
        }

        private void RemoveCache_Executed(UIElement element)
        {
            ThumbnailCache.Current.Remove();

            var dialog = new MessageDialog("", "キャッシュを削除しました");
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion
    }


    public class SettingPageVisualSlideshow : SettingPage
    {
        public SettingPageVisualSlideshow() : base("スライドショー")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsSlideShowByLoop))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsCancelSlideByMouseMove))),
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.SlideShowInterval)), new SlideShowInterval(), true)),
            };
        }
    }
}