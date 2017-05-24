﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Utility;
using NeeView.Windows.Controls;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace NeeView
{
    /// <summary>
    /// MainWindow : ViewModel
    /// TODO : モデルの分離
    /// </summary>
    public class MainWindowVM : BindableBase //, IDisposable
    {
        public static MainWindowVM Current { get; private set; }

        // フォーカス初期化要求
        ////public event EventHandler ResetFocus;



        // メニューを自動的に隠す
        #region Property: IsHideMenu
        private bool _isHideMenu;
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set
            {
                _isHideMenu = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHideMenu));
                UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }

        //
        public bool CanHideMenu => IsHideMenu || WindowShape.Current.IsFullScreen;

        #endregion

        // スライダーを自動的に隠す
        #region Property: IsHidePageSlider
        private bool _isIsHidePageSlider;
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set
            {
                _isIsHidePageSlider = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePageSlider));
                UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }

        //
        public bool CanHidePageSlider => IsHidePageSlider || WindowShape.Current.IsFullScreen;

        #endregion

        // パネルを自動的に隠す
        #region Property: IsHidePanel
        private bool _isHidePanel; // = true;
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set
            {
                _isHidePanel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePanel));
            }
        }

        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
        }

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        public bool IsHidePanelInFullscreen
        {
            get { return _IsHidePanelInFullscreen; }
            set { if (_IsHidePanelInFullscreen != value) { _IsHidePanelInFullscreen = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(CanHidePanel)); } }
        }

        //
        private bool _IsHidePanelInFullscreen;



        // パネルを自動的に隠せるか
        public bool CanHidePanel => IsHidePanel || (IsHidePanelInFullscreen && WindowShape.Current.IsFullScreen);

        #endregion


        /// <summary>
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        private bool _IsVisibleWindowTitle;
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set { if (_IsVisibleWindowTitle != value) { _IsVisibleWindowTitle = value; RaisePropertyChanged(); } }
        }


        // アドレスバーON/OFF
        #region Property: IsVisibleAddressBar
        private bool _isVisibleAddressBar;
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); }
        }
        public bool ToggleVisibleAddressBar()
        {
            IsVisibleAddressBar = !IsVisibleAddressBar;
            return IsVisibleAddressBar;
        }
        #endregion


        #region SidePanels

        /// <summary>
        /// SidePanelMargin property.
        /// メニュの自動非表示ON/OFFによるサイドパネル上部の余白
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, CanHideMenu ? 20 : 0, 0, CanHidePageSlider ? 20 : 0);
        }


        /// <summary>
        /// CanvasWidth property.
        /// キャンバスサイズ。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasHeight;

        #endregion


        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource _windowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource _windowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _windowIconDefault = null;
            _windowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource WindowIcon
            => SlideShow.Current.IsPlayingSlideShow ? _windowIconPlay : _windowIconDefault;

        #endregion




        #region テーマカラー

        //
        public void UpdatePanelColor()
        {
            if (App.Current == null) return;

            int alpha = _panelOpacity * 0xFF / 100;
            if (alpha > 0xff) alpha = 0xff;
            if (alpha < 0x00) alpha = 0x00;
            if (_model.PanelColor == PanelColor.Dark)
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x11, 0x11, 0x11));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x22, 0x22, 0x22));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x90, 0xEE, 0x90));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVFolderPen"] = null;
            }
            else
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xF8, 0xF8, 0xF8));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0x44));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                App.Current.Resources["NVFolderPen"] = new Pen(new SolidColorBrush(Color.FromRgb(0xDE, 0xB9, 0x82)), 1);
            }
        }

        // パネルの透明度(未使用)
        private int _panelOpacity = 100;
        public int PanelOpacity
        {
            get { return _panelOpacity; }
            set { _panelOpacity = value; UpdatePanelColor(); RaisePropertyChanged(); }
        }

        #endregion


        #region コンテキストメニュー

        //
        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { _contextMenu = value; RaisePropertyChanged(); }
        }

        public void UpdateContextMenu()
        {
            ContextMenu = _model.ContextMenuSetting.ContextMenu;
        }

        #endregion


        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }




        /// <summary>
        /// BusyVisibility property.
        /// アクセス中マーク表示用
        /// </summary>
        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set { if (_busyVisibility != value) { _busyVisibility = value; RaisePropertyChanged(); } }
        }

        private Visibility _busyVisibility;



        // for Binding
        public WindowShape WindowShape => WindowShape.Current;
        public WindowTitle WindowTitle => WindowTitle.Current;
        public ThumbnailList ThumbnailList => ThumbnailList.Current;
        public ContentCanvasBrush ContentCanvasBrush => ContentCanvasBrush.Current;
        public ImageEffect ImageEffect => ImageEffect.Current;
        public MouseInput MouseInput => MouseInput.Current;
        public InfoMessage InfoMessage => InfoMessage.Current;
        public SidePanel SidePanel => SidePanel.Current;
        public ContentCanvas ContentCanvas => ContentCanvas.Current;



        /// <summary>
        /// Model property.
        /// </summary>
        public MainWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private MainWindowModel _model;


        /// <summary>
        /// コンストラクター
        /// TODO: 引数はMainWindowModelであるべきでは？
        /// </summary>
        /// <param name="window"></param>
        public MainWindowVM(MainWindow window)
        {
            Current = this;

            // icon
            InitializeWindowIcons();


            // create Models
            Models.Instantiate();

            // mainwindow model
            _model = MainWindowModel.Current;

            _model.AddPropertyChanged(nameof(_model.PanelColor),
                (s, e) => UpdatePanelColor());

            _model.AddPropertyChanged(nameof(_model.ContextMenuSetting),
                (s, e) => UpdateContextMenu());

            // 初期化
            UpdatePanelColor();
            UpdateContextMenu();


            // Window Shape
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.Current.IsFullScreen),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(CanHidePanel));
                    UpdateSidePanelMargin();
                });

            // Side Panel
            ////SidePanel.Current.ResetFocus += (s, e) => ResetFocus?.Invoke(this, null);


            // SlideShow link to WindowIcon
            SlideShow.Current.AddPropertyChanged(nameof(SlideShow.IsPlayingSlideShow),
                (s, e) => RaisePropertyChanged(nameof(WindowIcon)));


            // JobEngine Busy
            JobEngine.Current.AddPropertyChanged(nameof(JobEngine.IsBusy),
                (s, e) => this.BusyVisibility = JobEngine.Current.IsBusy && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed);


            BookHub.Current.BookChanged +=
                (s, e) => CommandManager.InvalidateRequerySuggested();


            // TODO: アプリの初期化処理で行うべき
            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.TempDownloadDirectory);
            }
        }


        // 履歴削除
        // TODO: 直接変更し、最近使ったファイルはイベントで更新すべき
        public void ClearHistory()
        {
            BookHistory.Current.Clear();
            MenuBar.Current.UpdateLastFiles();
        }


        // 最後に開いたフォルダーを開く
        // 起動フローでの処理
        // TODO: ここではない。Model?
        public void LoadLastFolder()
        {
            if (!Preference.Current.bootup_lastfolder) return;

            string place = BookHistory.Current.LastAddress;
            if (place != null || System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
            {
                BookHub.Current.Load(place, BookLoadOption.Resume);
            }
        }


        // ジェスチャー表示
        // TODO: InfoMessageで面倒を見る？マウスからのイベントで。
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            InfoMessage.Current.SetMessage(
                InfoMessageType.Gesture,
                ((commandName != null) ? commandName + "\n" : "") + gesture,
                gesture + ((commandName != null) ? " " + commandName : ""));
        }



        #region クリップボード関連
        // TODO: ContentCanvas ?

        //
        private BitmapSource CurrentBitmapSource
        {
            get { return (ContentCanvas.Current.MainContent?.Content as BitmapContent)?.BitmapSource; }
        }

        //
        public bool CanCopyImageToClipboard()
        {
            return CurrentBitmapSource != null;
        }


        // クリップボードに画像をコピー
        public void CopyImageToClipboard()
        {
            try
            {
                if (CanCopyImageToClipboard())
                {
                    ClipboardUtility.CopyImage(CurrentBitmapSource);
                }
            }
            catch (Exception e)
            {
                new MessageDialog($"原因: {e.Message}", "コピーに失敗しました").ShowDialog();
            }
        }

        #endregion


        #region 印刷
        // TODO: ContentCanvas ? 

        /// <summary>
        /// 印刷可能判定
        /// </summary>
        /// <returns></returns>
        public bool CanPrint()
        {
            return ContentCanvas.Current.MainContent != null && ContentCanvas.Current.MainContent.IsValid;
        }

        /// <summary>
        /// 印刷
        /// </summary>
        public void Print(Window owner, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            // 掃除しておく
            GC.Collect();

            var contents = ContentCanvas.Current.Contents;
            var mainContent = ContentCanvas.Current.MainContent;

            // スケールモード退避
            var scaleModeMemory = contents.ToDictionary(e => e, e => e.BitmapScalingMode);

            // アニメーション停止
            foreach (var content in contents)
            {
                content.AnimationImageVisibility = Visibility.Visible;
                content.AnimationPlayerVisibility = Visibility.Collapsed;
            }

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            try
            {
                var context = new PrintContext();
                context.MainContent = mainContent;
                context.Contents = contents;
                context.View = element;
                context.ViewTransform = transform;
                context.ViewWidth = width;
                context.ViewHeight = height;
                context.ViewEffect = ImageEffect.Current.Effect;
                context.Background = ContentCanvasBrush.Current.CreateBackgroundBrush();
                context.BackgroundFront = ContentCanvasBrush.Current.CreateBackgroundFrontBrush(new DpiScale(1, 1));

                var dialog = new PrintWindow(context);
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
            finally
            {
                // スケールモード、アニメーション復元
                foreach (var content in contents)
                {
                    content.BitmapScalingMode = scaleModeMemory[content];
                    content.AnimationImageVisibility = Visibility.Collapsed;
                    content.AnimationPlayerVisibility = Visibility.Visible;
                }

                // スライドショー再開
                SlideShow.Current.ResumeSlideShow();
            }
        }

        #endregion




        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public bool IsLimitMove { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsControlCenterImage { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsAngleSnap { get; set; } // no used (ver.23)

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public double AngleFrequency { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsViewStartPositionCenter { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public PageStretchMode StretchMode { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public BackgroundStyle Background { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsSliderDirectionReversed { get; set; } // no used

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle CommandShowMessageStyle { get; set; } // no used (ver.22)

            [DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle GestureShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(Order = 1, EmitDefaultValue = false)]
            public bool IsEnabledNearestNeighbor { get; set; } // no used (ver.22)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepScale { get; set; } // no used(ver.23)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepAngle { get; set; }  // no used(ver.23)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsKeepFlip { get; set; } // no used(ver.23)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsLoadLastFolder { get; set; } // no used (ver.22)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsDisableMultiBoot { get; set; } // no used (ver.23)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsAutoPlaySlideShow { get; set; } // no used (ver.23)

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public bool IsSaveWindowPlacement { get; set; } // no used (ver.23)

            [DataMember(Order = 2)]
            public bool IsHideMenu { get; set; }

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsHideTitleBar { get; set; } // no used

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleTitleBar { get; set; } // no used (ver.22)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsSaveFullScreen { get; set; } // no used (ver.23)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsTopmost { get; set; } // no used (ver.22)

            [DataMember(Order = 5, EmitDefaultValue = false)]
            public FileInfoSetting FileInfoSetting { get; set; } // no used

            [DataMember(Order = 5, EmitDefaultValue = false)]
            public string UserDownloadPath { get; set; } // no used (ver.23)

            [DataMember(Order = 6, EmitDefaultValue = false)]
            public FolderListSetting FolderListSetting { get; set; } // no used

            [DataMember(Order = 6, EmitDefaultValue = false)]
            public PanelColor PanelColor { get; set; } // no used (ver.23)

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat1 { get; set; } // no used (ver.23)

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat2 { get; set; } // no used (ver.23)

            [DataMember(Order = 8)]
            public bool IsVisibleAddressBar { get; set; }

            [DataMember(Order = 8)]
            public bool IsHidePanel { get; set; }

            [DataMember(Order = 8)]
            public bool IsHidePanelInFullscreen { get; set; }

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public ContextMenuSetting ContextMenuSetting { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsEnableThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsHideThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public double ThumbnailSize { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsSliderLinkedThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailNumber { get; set; } // no used (ver.23)

            [DataMember(Order = 9, EmitDefaultValue = false)]
            public bool IsAutoGC { get; set; } // no used (ver.23)

            [DataMember(Order = 9, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailPlate { get; set; } // no used (ver.23)

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public bool IsOriginalScaleShowMessage { get; set; } // no used (ver.23)

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public double ContentsSpace { get; set; } // no used (ver.23)

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; } // no used (ver.23)

            [DataMember(Order = 16, EmitDefaultValue = false)]
            public SliderDirection SliderDirection { get; set; } // no used (ver.23)

            [DataMember(Order = 17)]
            public bool IsHidePageSlider { get; set; }

            [DataMember(Order = 18, EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; } // no used (ver.23)

            [DataMember(Order = 19)]
            public bool IsVisibleWindowTitle { get; set; }

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public bool IsVisibleLoupeInfo { get; set; } // no used (ver.23)

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsSliderWithIndex { get; set; } // no used

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsLoupeCenter { get; set; } // no used (ver.23)

            [DataMember(Order = 21, EmitDefaultValue = false)]
            public SliderIndexLayout SliderIndexLayout { get; set; } // no used (ver.23)

            [DataMember(Order = 21, EmitDefaultValue = false)]
            public BrushSource CustomBackground { get; set; } // no used (ver.23)

            //
            private void Constructor()
            {
                _Version = App.Config.ProductVersionNumber;
                ////IsSaveWindowPlacement = true;
                IsHidePanelInFullscreen = true;
                IsVisibleWindowTitle = true;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Config.GenerateProductVersionNumber(1, 16, 0))
                {
                    SliderDirection = IsSliderDirectionReversed ? SliderDirection.RightToLeft : SliderDirection.LeftToRight;
                }
                IsSliderDirectionReversed = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 17, 0))
                {
                    IsHidePageSlider = IsHideMenu;
                    IsHideMenu = false;
                }

                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    AngleFrequency = IsAngleSnap ? 45 : 0;
                }
                IsAngleSnap = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 21, 0))
                {
                    SliderIndexLayout = IsSliderWithIndex ? SliderIndexLayout.Right : SliderIndexLayout.None;
                }
                IsSliderWithIndex = false;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = App.Config.ProductVersionNumber;

            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsHidePanel = memento.IsHidePanel;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;

            var models = Models.Current;

            // compatible before ver.22
            if (memento._Version < Config.GenerateProductVersionNumber(1, 22, 0))
            {
                if (memento.FileInfoSetting != null)
                {
                    models.FileInformation.IsUseExifDateTime = memento.FileInfoSetting.IsUseExifDateTime;
                    models.FileInformation.IsVisibleBitsPerPixel = memento.FileInfoSetting.IsVisibleBitsPerPixel;
                    models.FileInformation.IsVisibleLoader = memento.FileInfoSetting.IsVisibleLoader;
                }
                if (memento.FolderListSetting != null)
                {
                    models.FolderList.IsVisibleBookmarkMark = memento.FolderListSetting.IsVisibleBookmarkMark;
                    models.FolderList.IsVisibleHistoryMark = memento.FolderListSetting.IsVisibleHistoryMark;
                }

                models.InfoMessage.CommandShowMessageStyle = memento.CommandShowMessageStyle;

                WindowShape.Current.IsTopmost = memento.IsTopmost;
                WindowShape.Current.IsCaptionVisible = memento.IsVisibleTitleBar;
                Preference.Current.bootup_lastfolder = memento.IsLoadLastFolder;
            }

            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                Preference.Current.download_path = memento.UserDownloadPath;

                _model.PanelColor = memento.PanelColor;
                _model.ContextMenuSetting = memento.ContextMenuSetting;

                models.MemoryControl.IsAutoGC = memento.IsAutoGC;

                models.InfoMessage.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
                models.InfoMessage.GestureShowMessageStyle = memento.GestureShowMessageStyle;
                models.InfoMessage.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
                models.InfoMessage.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;

                models.SlideShow.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;

                models.ContentCanvasTransform.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
                models.ContentCanvasTransform.IsLimitMove = memento.IsLimitMove;
                models.ContentCanvasTransform.AngleFrequency = memento.AngleFrequency;
                models.ContentCanvasTransform.IsControlCenterImage = memento.IsControlCenterImage;
                models.ContentCanvasTransform.IsKeepAngle = memento.IsKeepAngle;
                models.ContentCanvasTransform.IsKeepFlip = memento.IsKeepFlip;
                models.ContentCanvasTransform.IsKeepScale = memento.IsKeepScale;
                models.ContentCanvasTransform.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;

                models.ContentCanvas.StretchMode = memento.StretchMode;
                models.ContentCanvas.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
                models.ContentCanvas.ContentsSpace = memento.ContentsSpace;
                models.ContentCanvas.IsAutoRotate = memento.IsAutoRotate;

                models.ContentCanvasBrush.CustomBackground = memento.CustomBackground;
                models.ContentCanvasBrush.Background = memento.Background;

                models.WindowTitle.WindowTitleFormat1 = memento.WindowTitleFormat1;
                models.WindowTitle.WindowTitleFormat2 = memento.WindowTitleFormat2;

                models.PageSlider.SliderIndexLayout = memento.SliderIndexLayout;
                models.PageSlider.SliderDirection = memento.SliderDirection;
                models.PageSlider.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;

                models.ThumbnailList.IsEnableThumbnailList = memento.IsEnableThumbnailList;
                models.ThumbnailList.IsHideThumbnailList = memento.IsHideThumbnailList;
                models.ThumbnailList.ThumbnailSize = memento.ThumbnailSize;
                models.ThumbnailList.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
                models.ThumbnailList.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;

                models.MouseInput.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
                models.MouseInput.IsLoupeCenter = memento.IsLoupeCenter;
                models.MouseInput.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            }
        }

        #endregion
    }
}
