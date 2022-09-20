using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Media.Imaging;
using NeeView.Properties;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace NeeView
{
    /// <summary>
    /// 回転の初期化モード
    /// </summary>
    public enum AngleResetMode
    {
        /// <summary>
        /// 現在の角度を維持
        /// </summary>
        None,

        /// <summary>
        /// 通常。AutoRotateするかを判定し角度を求める
        /// </summary>
        Normal,

        /// <summary>
        /// AutoRotateの角度を強制適用する
        /// </summary>
        ForceAutoRotate,
    }

    // 自動回転タイプ
    public enum AutoRotateType
    {
        [AliasName]
        None,

        [AliasName]
        Left,

        [AliasName]
        Right,
    }

    public static class AutoRotateTypeExtensions
    {
        public static double ToAngle(this AutoRotateType self)
        {
            return self switch
            {
                AutoRotateType.Left => -90.0,
                AutoRotateType.Right => 90.0,
                _ => 0.0,
            };
        }
    }

    /// <summary>
    /// ページ表示コンテンツ管理
    /// </summary>
    public class ContentCanvas : BindableBase, IDisposable
    {

        private readonly object _lock = new();
        private readonly MainViewComponent _viewComponent;
        private readonly ContentSizeCalcurator _contentSizeCalcurator;
        private PageStretchMode _stretchModePrev = PageStretchMode.Uniform;
        private double _baseScale;
        private double _lastScale;
        private readonly DpiScaleProvider _dpiProvider;
        private Size _viewSize;
        private readonly DisposableCollection _disposables = new();

        public ContentCanvas(MainViewComponent viewComponent, BookHub bookHub)
        {
            if (viewComponent is null) throw new ArgumentNullException(nameof(viewComponent));
            if (viewComponent.MainView?.DpiProvider is null) throw new ArgumentException("viewComponent must have DpiProvicer");

            _viewComponent = viewComponent;
            _dpiProvider = viewComponent.MainView.DpiProvider;

            _disposables.Add(_dpiProvider.SubscribeDpiChanged(
                (s, e) => DpiChanged?.Invoke(s, e)));

            _contentSizeCalcurator = new ContentSizeCalcurator();

            _disposables.Add(_viewComponent.DragTransform.SubscribeTransformChanged(
                Transform_TransformChanged));
            _disposables.Add(_viewComponent.LoupeTransform.SubscribeTransformChanged(
                Transform_TransformChanged));

            // Contents
            Contents = new ObservableCollection<ViewContent>
            {
                new ViewContent(),
                new ViewContent()
            };

            MainContent = Contents[0];

            _disposables.Add(bookHub.SubscribeBookChanging(
                (s, e) => AppDispatcher.Invoke(() => IgnoreViewContentsReservers())));

            // TODO: BookOperationから？
            _disposables.Add(bookHub.SubscribeViewContentsChanged(
                (s, e) => AppDispatcher.Invoke(() => OnViewContentsChanged(s, e))));

            // NOTE: NextContentsChangedのリサイズ処理は非同期。MagicScaler(WIC)が非同期のみをサポート
            _disposables.Add(bookHub.SubscribeNextContentsChanged(
                (s, e) => OnNextContentsChanged(s, e)));

            _disposables.Add(bookHub.SubscribeEmptyMessage(
                (s, e) => AppDispatcher.Invoke(() => EmptyPageMessage = e.Message)));

            _disposables.Add(bookHub.SubscribeEmptyPageMessage(
                (s, e) => AppDispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(e.Message))
                    {
                        EmptyPageMessage = e.Message;
                        IsVisibleEmptyPageMessage = true;
                    }
                })));

            _disposables.Add(Config.Current.ImageDotKeep.SubscribePropertyChanged(nameof(ImageDotKeepConfig.IsEnabled),
                (s, e) => UpdateContentScalingMode()));

            _disposables.Add(Config.Current.Book.SubscribePropertyChanged(nameof(BookConfig.ContentsSpace),
                (s, e) => UpdateContentSize()));

            _disposables.Add(Config.Current.View.SubscribePropertyChanging(
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(ViewConfig.StretchMode):
                            _stretchModePrev = Config.Current.View.StretchMode;
                            break;
                    }
                }));

            _disposables.Add(Config.Current.View.SubscribePropertyChanged(
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(ViewConfig.StretchMode):
                            Stretch();
                            break;

                        case nameof(ViewConfig.AllowStretchScaleUp):
                        case nameof(ViewConfig.AllowStretchScaleDown):
                        case nameof(ViewConfig.IsBaseScaleEnabled):
                        case nameof(ViewConfig.BaseScale):
                            ResetContentSize();
                            break;

                        case nameof(ViewConfig.AutoRotate):
                            RaisePropertyChanged(nameof(IsAutoRotateLeft));
                            RaisePropertyChanged(nameof(IsAutoRotateRight));
                            ResetContentSizeAndTransform(new ResetTransformCondition(true));
                            break;
                    }
                }));
        }


        /// <summary>
        /// コンテンツ変更通知
        /// </summary>
        public event EventHandler? ContentChanged;

        public IDisposable SubscribeContentChanged(EventHandler handler)
        {
            ContentChanged += handler;
            return new AnonymousDisposable(() => ContentChanged -= handler);
        }

        /// <summary>
        /// コンテンツサイズ変更通知
        /// </summary>
        public event EventHandler? ContentSizeChanged;

        public IDisposable SubscribeContentSizeChanged(EventHandler handler)
        {
            ContentSizeChanged += handler;
            return new AnonymousDisposable(() => ContentSizeChanged -= handler);
        }

        /// <summary>
        /// コンテンツストレッチ変更通知
        /// </summary>
        public event EventHandler? ContentStretchChanged;

        public IDisposable SubscribeContentStretchChanged(EventHandler handler)
        {
            ContentStretchChanged += handler;
            return new AnonymousDisposable(() => ContentStretchChanged -= handler);
        }

        /// <summary>
        /// DPI変更通知
        /// </summary>
        public event EventHandler? DpiChanged;

        public IDisposable SubscribeDpiChanged(EventHandler handler)
        {
            DpiChanged += handler;
            return new AnonymousDisposable(() => DpiChanged -= handler);
        }


        // 空フォルダー通知表示のON/OFF
        private bool _isVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _isVisibleEmptyPageMessage; }
            set { SetProperty(ref _isVisibleEmptyPageMessage, value && Config.Current.Notice.IsEmptyMessageEnabled); }
        }

        // 空フォルダー通知表示の詳細テキスト
        private string? _emptyPageMessage;
        public string? EmptyPageMessage
        {
            get { return _emptyPageMessage; }
            set { _emptyPageMessage = value; RaisePropertyChanged(); }
        }

        public bool IsAutoRotateLeft
        {
            get { return Config.Current.View.AutoRotate == AutoRotateType.Left; }
            set
            {
                if (value)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.Left;
                }
                else if (Config.Current.View.AutoRotate == AutoRotateType.Left)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.None;
                }
            }
        }

        public bool IsAutoRotateRight
        {
            get { return Config.Current.View.AutoRotate == AutoRotateType.Right; }
            set
            {
                if (value)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.Right;
                }
                else if (Config.Current.View.AutoRotate == AutoRotateType.Right)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.None;
                }
            }
        }

        // ビューエリアサイズ
        public Size ViewSize
        {
            get { return _viewSize; }
            private set
            {
                if (SetProperty(ref _viewSize, value))
                {
                    UpdateFixedViewSize();
                }
            }

        }

        // ビューエリアサイズ(計算用)
        public Size FixedViewSize { get; private set; }

        // コンテンツ
        public ObservableCollection<ViewContent> Contents { get; private set; }

        // コンテンツ複製。処理時のコレクション変更の例外を避けるため。
        public List<ViewContent> CloneContents
        {
            get
            {
                lock (_lock)
                {
                    return Contents.ToList();
                }
            }
        }

        // 見開き時のメインとなるコンテンツ
        private ViewContent? _mainContent;
        public ViewContent? MainContent
        {
            get { return _mainContent; }
            set
            {
                if (_mainContent != value)
                {
                    _mainContent = value;
                    RaisePropertyChanged();

                    this.IsMediaContent = _mainContent is MediaViewContent;
                }
            }
        }

        // メインコンテンツがメディアコンテンツ？
        private bool _isMediaContent;
        public bool IsMediaContent
        {
            get { return _isMediaContent; }
            set { if (_isMediaContent != value) { _isMediaContent = value; RaisePropertyChanged(); } }
        }


        // コンテンツマージン
        private Thickness _contentsMargin;
        public Thickness ContentsMargin
        {
            get { return _contentsMargin; }
            set { _contentsMargin = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 次のページ更新時の表示開始位置
        /// TODO: ちゃんとBookから情報として上げるようにするべき
        /// </summary>
        public DragViewOrigin NextViewOrigin { get; set; }

        /// <summary>
        /// ContentAngle property.
        /// </summary>
        private double _contentAngle;
        public double ContentAngle
        {
            get { return _contentAngle; }
            set { if (_contentAngle != value) { _contentAngle = value; RaisePropertyChanged(); } }
        }

        // メインコンテンツのオリジナル表示スケール
        public double MainContentScale => MainContent != null ? MainContent.Scale * _dpiProvider.DpiScale.ToFixedScale().DpiScaleX : 0.0;

        // コンテンツ(代表)のオリジナル表示スケール
        public double ContentScale => _baseScale * _dpiProvider.DpiScale.ToFixedScale().DpiScaleX;

        public GridLine GridLine { get; private set; } = new GridLine();

        public DpiScale Dpi => _dpiProvider.DpiScale.ToFixedScale();


        /// <summary>
        /// 角度設定モードを取得
        /// </summary>
        /// <param name="precedeAutoRotate">AutoRotate設定を優先する</param>
        /// <returns></returns>
        private static AngleResetMode GetAngleResetMode(bool precedeAutoRotate)
        {
            if (Config.Current.View.IsKeepAngle)
            {
                if (precedeAutoRotate)
                {
                    if (Config.Current.View.AutoRotate != AutoRotateType.None)
                    {
                        return AngleResetMode.ForceAutoRotate;
                    }
                    else
                    {
                        return AngleResetMode.Normal;
                    }
                }
                else
                {
                    return AngleResetMode.None;
                }
            }
            else
            {
                return AngleResetMode.Normal;
            }
        }


        // トランスフォーム変更イベント処理
        private void Transform_TransformChanged(object? sender, TransformEventArgs e)
        {
            UpdateContentScalingMode();
            _viewComponent.MouseInput.ShowMessage(e.ActionType, MainContent);

            if (e.ActionType == TransformActionType.Angle)
            {
                var result = _contentSizeCalcurator.GetFixedContentSize(GetContentSizeList(), FixedViewSize, _viewComponent.DragTransform.Angle, Dpi);
                _lastScale = result.GetScale();
            }
        }

        // コンテンツカラー
        public Color GetContentColor()
        {
            return Contents[Contents[1].IsValid ? 1 : 0].Color;
        }

        // 現在のビューコンテンツのリザーバーを無効化
        private void IgnoreViewContentsReservers()
        {
            foreach (var content in CloneContents)
            {
                content.IgnoreReserver = true;
            }
        }

        /// <summary>
        /// 表示コンテンツ更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            var contents = new List<ViewContent>();

            // ViewContent作成
            if (e?.ViewPageCollection?.Collection != null)
            {
                foreach (var source in e.ViewPageCollection.Collection)
                {
                    if (source != null)
                    {
                        var old = Contents[contents.Count];
                        var content = ViewContentFactory.Create(_viewComponent, source, old);
                        contents.Add(content);
                    }
                }
            }

            // ページが存在しない場合、専用メッセージを表示する
            IsVisibleEmptyPageMessage = e?.ViewPageCollection != null && contents.Count == 0;

            // メインとなるコンテンツを指定
            var validContents = contents.Where(x => !x.IsDummy).ToList();
            var mainContent = validContents.Count > 0 ? (validContents.First().Position < validContents.Last().Position ? validContents.First() : validContents.Last()) : null;

            // ViewModelプロパティに反映
            lock (_lock)
            {
                var oldies = new List<ViewContent>(Contents);

                for (int index = 0; index < 2; ++index)
                {
                    Contents[index] = index < contents.Count ? contents[index] : new ViewContent();
                }

                MainContent = mainContent;

                foreach (var content in oldies.Except(Contents))
                {
                    content.OnDetached();
                }

                foreach (var content in Contents.Except(oldies))
                {
                    content.OnAttached();
                }

                UpdateFixedViewSize();
            }

            // ルーペ解除
            if (Config.Current.Loupe.IsResetByPageChanged)
            {
                _viewComponent.ViewController.SetLoupeMode(false);
            }

            // 回転後のページ移動のスケール維持補正
            if (Config.Current.View.IsKeepScale && _baseScale != _lastScale)
            {
                // TODO: DragTransformControl経由にせよ
                var scaleRate = _baseScale / _lastScale;
                _viewComponent.DragTransform.SetScale(_viewComponent.DragTransform.Scale * scaleRate, TransformActionType.None);
            }

            var isNewBook = e is null || e.IsFirst;

            // コンテンツサイズ更新
            var angleResetMode = GetAngleResetMode(isNewBook && !(Config.Current.View.IsKeepAngle && Config.Current.View.IsKeepAngleBooks));
            UpdateContentSize(GetAutoRotateAngle(angleResetMode));

            // リザーブコンテンツでなければ座標初期化
            // HACK: ルーペ時の挙動があやしい
            bool isReserveContent = e?.ViewPageCollection?.Collection?.Any(x => x.GetContentType() == ViewContentType.Reserve) ?? false;
            if (!isReserveContent)
            {
                ResetTransform(e != null ? e.ViewPageCollection.Range.Direction : 0, NextViewOrigin, angleResetMode, ResetTransformCondition.Create(isNewBook));
                NextViewOrigin = DragViewOrigin.None;
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);

            // GC
            MemoryControl.Current.GarbageCollect();

            ////DebugTimer.Check("UpdatedContentCanvas");
        }

        // 表示物として有効なコンテンツ？
        public bool IsViewContents()
        {
            return Contents.Any(e => e.IsViewContent);
        }

        // 先読みコンテンツ更新
        // 表示サイズを確定し、フィルター適用時にリサイズ処理を行う
        private void OnNextContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs source)
        {
            if (source?.ViewPageCollection?.Collection == null) return;

            bool includeLoupeScale = _viewComponent.LoupeTransform.IsEnabled && !Config.Current.Loupe.IsResetByPageChanged;
            ResizeConten(source.ViewPageCollection, includeLoupeScale, CancellationToken.None);
        }


        /// <summary>
        /// コンテンツリサイズ
        /// </summary>
        private void ResizeConten(ViewContentSourceCollection viewPageCollection, bool includeLoupeScale, CancellationToken token)
        {
            if (viewPageCollection?.Collection == null) return;

            token.ThrowIfCancellationRequested();


            var sizes = viewPageCollection.Collection.Select(e => e.Size).ToList();
            while (sizes.Count < 2)
            {
                sizes.Add(SizeExtensions.Zero);
            }

            // 表示サイズ計算
            var result = MainContent is MediaViewContent
                ? _contentSizeCalcurator.GetFixedContentSize(sizes, FixedViewSize, 0.0, Dpi)
                : _contentSizeCalcurator.GetFixedContentSize(sizes, FixedViewSize, GetAngleResetMode(false), _viewComponent.DragTransform.Angle, Dpi);

            // 表示スケール推定
            var scale = (Config.Current.View.IsKeepScale ? _viewComponent.DragTransform.Scale : 1.0) * (includeLoupeScale ? _viewComponent.LoupeTransform.FixedScale : 1.0) * _dpiProvider.DpiScale.DpiScaleX;

            // リサイズ
            for (int i = 0; i < 2; ++i)
            {
                var size0 = sizes[i];
                if (size0.IsZero()) continue;

                var size1 = result.ContentSizeList[i].Multi(scale);
                if (i < viewPageCollection.Collection.Count && viewPageCollection.Collection[i].IsHalf) // 分割前サイズでリサイズ
                {
                    size1 = new Size(size1.Width * 2.0, size1.Height);
                }
                ////Debug.WriteLine($"{i}: {size0} => {size1.Truncate()}");


                var content = viewPageCollection.Collection[i].Content;
                try
                {
                    if (content.PageMessage == null && content.CanResize && content is BitmapContent bitmapContent)
                    {
                        var dispSize = new Size(size1.Width, size1.Height);
                        var resized = bitmapContent.Picture?.CreateImageSource(bitmapContent.GetRenderSize(dispSize), token);
                        if (resized == true)
                        {
                            viewPageCollection.Collection[i].Page.DebugRaiseContentPropertyChanged();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnNextContentChanged: " + ex.Message);
                    content.SetPageMessage(ex);
                }
            }
        }

        /// <summary>
        /// コンテンツサイズを初期化
        /// </summary>
        public void ResetContentSize()
        {
            if (_disposedValue) return;

            UpdateContentSize();
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
            ResetTransformRaw(true, false, false, 0.0, false);
        }

        /// <summary>
        /// コンテンツサイズと座標系を初期化
        /// </summary>
        public void ResetContentSizeAndTransform(ResetTransformCondition condition)
        {
            if (_disposedValue) return;

            var angleResetMode = GetAngleResetMode(true);

            UpdateContentSize(GetAutoRotateAngle(angleResetMode));
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
            ResetTransform(0, DragViewOrigin.None, angleResetMode, condition);
        }

        // 座標系初期化
        public void ResetTransform(int pageDirection, DragViewOrigin viewOrigin, AngleResetMode angleResetMode, ResetTransformCondition condition)
        {
            if (_disposedValue) return;

            // NOTE: ルーペモードのときは初期化しない
            if (_viewComponent.IsLoupeMode) return;

            _viewComponent.DragTransformControl.SetMouseDragSetting(pageDirection, viewOrigin, BookSettingPresenter.Current.LatestSetting.BookReadOrder);

            bool isResetScale = condition.IsResetScale;
            bool isResetAngle = condition.IsResetAngle || angleResetMode != AngleResetMode.None;
            bool isResetFlip = condition.IsResetFlip;
            ResetTransformRaw(isResetScale, isResetAngle, isResetFlip, GetAutoRotateAngle(angleResetMode), false);
        }

        /// <summary>
        /// 座標系の初期化。
        /// フラグに関係なく移動は初期化される
        /// </summary>
        /// <param name="isResetScale">スケールを初期化する</param>
        /// <param name="isResetAngle">角度をangleで初期化する</param>
        /// <param name="isResetFlip">反転を初期化する</param>
        /// <param name="angle">角度初期化の値</param>
        /// <param name="ignoreViewOrigin">初期中心座標補正無しで初期化。座標が必ず0,0になる</param>
        public void ResetTransformRaw(bool isResetScale, bool isResetAngle, bool isResetFlip, double angle, bool ignoreViewOrigin)
        {
            if (_disposedValue) return;

            _viewComponent.DragTransformControl.Reset(isResetScale, isResetAngle, isResetFlip, angle, ignoreViewOrigin);

            if (Config.Current.Mouse.IsHoverScroll)
            {
                _viewComponent.DragTransformControl.HoverScroll(Mouse.GetPosition(_viewComponent.DragTransformControl.SenderElement), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// ページ開始時の回転
        /// </summary>
        /// <returns></returns>
        public double GetAutoRotateAngle(AngleResetMode angleResetMode)
        {
            if (_disposedValue) return 0.0;

            if (angleResetMode == AngleResetMode.None)
            {
                return _viewComponent.DragTransform.Angle;
            }
            else if (MainContent is MediaViewContent)
            {
                return 0.0;
            }
            else
            {
                return _contentSizeCalcurator.GetAutoRotateAngle(GetContentSizeList(), FixedViewSize, angleResetMode, _viewComponent.DragTransform.Angle);
            }
        }

        /// <summary>
        /// 有効な表示コンテンツサイズのリストを取得
        /// </summary>
        /// <returns></returns>
        private List<Size> GetContentSizeList()
        {
            return CloneContents.Select(e => (e.Source?.Size ?? SizeExtensions.Zero).EmptyOrZeroCoalesce(GetViewContentSize(e))).ToList();
        }

        // TODO: ViewContent.Size の廃止
        private static Size GetViewContentSize(ViewContent viewContent)
        {
            return viewContent.Size;
        }

        // ビューエリアサイズを更新
        public void SetViewSize(double width, double height)
        {
            if (_disposedValue) return;

            this.ViewSize = new Size(width, height);

            UpdateContentSize();

            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 計算用ビューエリアサイズを更新
        /// </summary>
        /// <remarks>
        /// ビューエリア縦幅丁度にするとSVG画像が描画されないことがある謎の現象を回避するために縦幅-1。
        /// この現象はストレッチモードを縦幅依存のものにしたときに発生する。ウィンドウ最大化で発生しやすい。
        /// 現象の発生した値を固定にしても発生せず、現象が発生しているときに表示要素を再接続するだけで表示されるので、フレームワーク依存のなんらかのタイミング不具合と思われる。
        /// この対策は現象の頻度を下げるだけで、根本の対策になっていない。
        /// </remarks>
        private void UpdateFixedViewSize()
        {
            lock (_lock)
            {
                if (Contents.Any(e => e is BitmapViewContent bitmapViewContent && bitmapViewContent.IsSvg))
                {
                    this.FixedViewSize = new Size(this.ViewSize.Width, Math.Max(this.ViewSize.Height - 1.0, 0.0));
                }
                else
                {
                    this.FixedViewSize = this.ViewSize;
                }
            }
        }

        private void UpdateContentSize(double angle)
        {
            this.ContentAngle = angle;
            UpdateContentSize();
        }

        // コンテンツ表示サイズを更新
        public void UpdateContentSize()
        {
            if (_disposedValue) return;
            if (!CloneContents.Any(e => e.IsValid)) return;

            var result = _contentSizeCalcurator.GetFixedContentSize(GetContentSizeList(), FixedViewSize, this.ContentAngle, Dpi);

            this.ContentsMargin = result.ContentsMargin;
            _lastScale = _baseScale = result.GetScale();

            for (int i = 0; i < 2; ++i)
            {
                if (Contents[i] is ArchiveViewContent)
                {
                    Contents[i].Width = 64;
                    Contents[i].Height = 64;
                }
                else
                {
                    Contents[i].Width = result.ContentSizeList[i].Width;
                    Contents[i].Height = result.ContentSizeList[i].Height;
                }
            }

            UpdateContentScalingMode();

            this.GridLine.SetSize(result.Width, result.Height);
        }


        // コンテンツスケーリングモードを更新
        public void UpdateContentScalingMode(ViewContent? target = null)
        {
            if (_disposedValue) return;

            double finalScale = _viewComponent.DragTransform.Scale * _viewComponent.LoupeTransform.FixedScale * _dpiProvider.DpiScale.DpiScaleX;

            foreach (var content in CloneContents)
            {
                if (target != null && target != content) continue;

                if (content.View != null && content.Source != null && content.IsBitmapScalingModeSupported)
                {
                    if (content is not BitmapViewContent bitmapContent) continue;

                    var image = bitmapContent.GetViewImage();
                    if (image == null) continue;

                    var viewBox = content.Source.GetViewBox();
                    var pixelHeight = (int)(image.GetPixelHeight() * viewBox.Height);
                    var pixelWidth = (int)(image.GetPixelWidth() * viewBox.Width);
                    var viewHeight = content.Height * finalScale;
                    var viewWidth = content.Width * finalScale;

                    ContentViewMode viewMode;
                    var diff = Math.Abs(pixelWidth - viewWidth);
                    var diffAngle = Math.Abs(_viewComponent.DragTransform.Angle % 90.0);
                    if (diff < 1.1 && diffAngle < 0.1)
                    {
                        content.BitmapScalingMode = BitmapScalingMode.NearestNeighbor;
                        viewMode = Config.Current.ImageTrim.IsEnabled ? ContentViewMode.Scale : ContentViewMode.Pixeled;
                    }
                    else
                    {
                        var isImageDotKeep = Config.Current.ImageDotKeep.IsImgeDotKeep(new Size(viewWidth, viewHeight), new Size(pixelWidth, pixelHeight));
                        content.BitmapScalingMode = isImageDotKeep ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
                        viewMode = ContentViewMode.Scale;
                    }
                    content.SetViewMode(viewMode, finalScale);

                    // ##
                    DebugInfo.Current?.SetMessage($"{content.BitmapScalingMode}: s={pixelHeight}: v={viewHeight:0.00}: a={_viewComponent.DragTransform.Angle:0.00}");

                    if (bitmapContent.IsDarty())
                    {
                        ContentSizeChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        #region スケールモード

        // トグル
        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = Config.Current.View.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var next = (int)mode + 1;
                if (!param.IsLoop && next >= length) return Config.Current.View.StretchMode;
                mode = (PageStretchMode)(next % length);
                if (param.GetStretchModeDictionary()[mode]) return mode;
            }
            while (count++ < length);
            return Config.Current.View.StretchMode;
        }

        // 逆トグル
        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = Config.Current.View.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var prev = (int)mode - 1;
                if (!param.IsLoop && prev < 0) return Config.Current.View.StretchMode;
                mode = (PageStretchMode)((prev + length) % length);
                if (param.GetStretchModeDictionary()[mode]) return mode;
            }
            while (count++ < length);
            return Config.Current.View.StretchMode;
        }

        public PageStretchMode GetStretchMode()
        {
            return Config.Current.View.StretchMode;
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            if (_disposedValue) return;

            Config.Current.View.StretchMode = GetFixedStretchMode(mode, isToggle);
            Stretch();
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return mode == GetFixedStretchMode(mode, isToggle);
        }

        private PageStretchMode GetFixedStretchMode(PageStretchMode mode, bool isToggle)
        {
            if (isToggle && Config.Current.View.StretchMode == mode)
            {
                return (mode == PageStretchMode.None) ? _stretchModePrev : PageStretchMode.None;
            }
            else
            {
                return mode;
            }
        }

        #endregion

        #region 回転コマンド

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            if (_disposedValue) return;

            _viewComponent.DragTransformControl.Rotate(-parameter.Angle);

            if (parameter.IsStretch)
            {
                Stretch();
            }
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            if (_disposedValue) return;

            _viewComponent.DragTransformControl.Rotate(+parameter.Angle);

            if (parameter.IsStretch)
            {
                Stretch();
            }
        }

        public void Stretch(bool ignoreViewOrigin = false)
        {
            if (_disposedValue) return;

            UpdateContentSize(_viewComponent.DragTransform.Angle);
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
            ContentStretchChanged?.Invoke(this, EventArgs.Empty);
            ResetTransformRaw(true, false, false, 0.0, ignoreViewOrigin);
        }

        #endregion

        #region クリップボード関連

        private ImageSource? CurrentImageSource
        {
            get { return (this.MainContent?.Content as BitmapContent)?.ImageSource; }
        }

        public bool CanCopyImageToClipboard()
        {
            return CurrentImageSource is BitmapSource;
        }

        public void CopyImageToClipboard()
        {
            if (_disposedValue) return;

            try
            {
                if (CanCopyImageToClipboard() && CurrentImageSource is BitmapSource bitmapSource)
                {
                    ClipboardUtility.CopyImage(bitmapSource);
                }
            }
            catch (Exception e)
            {
                new MessageDialog($"{Resources.Word_Cause}: {e.Message}", Resources.CopyImageErrorDialog_Title).ShowDialog();
            }
        }

        #endregion


        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    if (this.Contents != null)
                    {
                        foreach (var content in this.Contents)
                        {
                            content.Dispose();
                        }
                    }
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


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [Obsolete("no used"), DataMember(Name = "StretchMode", EmitDefaultValue = false)]
            public PageStretchModeV1 StretchModeV1 { get; set; }

            [DataMember(Name = "StretchModeV2")]
            public PageStretchMode StretchMode { get; set; }


            [DataMember, DefaultValue(true)]
            public bool AllowEnlarge { get; set; }

            [DataMember, DefaultValue(true)]
            public bool AllowReduce { get; set; }

            [DataMember]
            public bool IsEnabledNearestNeighbor { get; set; }

            [DataMember]
            public double ContentsSpace { get; set; }

            [DataMember]
            public AutoRotateType AutoRotateType { get; set; }

            [DataMember]
            public GridLine.Memento? GridLine { get; set; }


            [Obsolete("no used"), DataMember(EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612, CS0618
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    AutoRotateType = IsAutoRotate ? AutoRotateType.Right : AutoRotateType.None;
                }

                // before 35.0
                if (_Version < Environment.GenerateProductVersionNumber(35, 0, 0))
                {
                    StretchMode = StretchModeV1.ToPageStretchMode();
                }
#pragma warning restore CS0612, CS0618
            }

            public void RestoreConfig(Config config)
            {
                config.ImageDotKeep.IsEnabled = IsEnabledNearestNeighbor;
                config.Book.ContentsSpace = ContentsSpace;

                config.View.StretchMode = StretchMode;
                config.View.AllowStretchScaleUp = AllowEnlarge;
                config.View.AllowStretchScaleDown = AllowReduce;
                config.View.AutoRotate = AutoRotateType;

                this.GridLine?.RestoreConfig(config);
            }
        }

        #endregion
    }
}
