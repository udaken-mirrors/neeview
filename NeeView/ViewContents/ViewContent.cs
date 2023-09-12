using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView;
using System.Windows.Input;
using System.Windows.Threading;
using NeeView.Windows;
using NeeLaboratory.Generators;
using System.Windows.Data;
using System.Windows.Shapes;

namespace NeeView
{

    public abstract partial class ViewContent : ContentControl, IDisposable 
    {
        private bool _initialized;
        private PageFrameElement _element;
        private PageFrameElementScale _scale;
        private readonly PageFrameActivity _activity;
        private readonly ViewSource _viewSource;
        private readonly ViewContentSize _viewContentSize;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private readonly SizeSource _sizeSource;
        private readonly PageBackgroundSource _backgroundSource;



        public ViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource)
        {
            _element = element;
            _scale = scale;
            _viewSource = viewSource;
            _activity = activity;

            _viewContentSize = ViewContentSizeFactory.Create(element, scale);

            _sizeSource = new SizeSource(LayoutSize);
            _sizeSource.BindTo(this);

            _backgroundSource = backgroundSource;
        }


        [Subscribable]
        public event EventHandler<ViewContentChangedEventArgs>? ViewContentChanged;


        public Page Page => _element.Page;
        public PageFrameActivity Activity => _activity;
        public ArchiveEntry ArchiveEntry => _element.Page.Entry;
        public PageFrameElement Element => _element;
        public ViewContentSize ViewContentSize => _viewContentSize;
        public Size LayoutSize => _viewContentSize.LayoutSize;
        public ViewSource ViewSource => _viewSource;
        public ViewContentState State { get; private set; }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    (Content as IDisposable)?.Dispose();
                    Content = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public virtual void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _disposables.Add(_viewSource.SubscribeDataSourceChanged(ViewSource_DataChanged));
            _disposables.Add(_element.Page.SubscribeContentChanged(AppDispatcher.BeginInvokeHandler(Page_ContentChanged)));

            UpdateContent(_viewSource.DataSource);

            Debug.WriteLine($"InitializeContentAsync: {ArchiveEntry}");
            RequestLoadViewSource(CancellationToken.None);
        }


        /// <summary>
        /// Source情報(PageFrameElement)の更新。
        /// </summary>
        /// <param name="element">互換性のあるPageFrameElement</param>
        /// <param name="scale">表示のスケール情報</param>
        /// <param name="force">強制更新</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetSource(PageFrameElement element, PageFrameElementScale scale, bool force)
        {
            if (_disposedValue) return;

            if (!_element.IsMatch(element)) throw new ArgumentException("Resources do not match");
            if (!force && _element.Equals(element) && _scale == scale) return;

            _element = element;
            _scale = scale;
            _viewContentSize.SetSource(_element, scale);

            // 強制更新であれば読み込み前に一度コンテンツ生成をしておく
            if (force)
            {
                UpdateContent(_viewSource.DataSource);
            }
            else
            {
                if (UpdateSize())
                {
                    ViewContentChanged?.Invoke(this, new ViewContentChangedEventArgs(ViewContentChangedAction.Size, this));
                }
            }

            //Debug.WriteLine($"OnSourceChanged: {ArchiveEntry} ({LayoutSize})");
            OnSourceChanged();
        }

        protected virtual void OnSourceChanged()
        {
        }


        private void Page_ContentChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            if (_element.Page.Content.IsLoaded)
            {
                //Debug.WriteLine($"OnContentChanged: {ArchiveEntry}");
                OnContentChanged();
            }
        }

        protected virtual void OnContentChanged()
        {
            RequestLoadViewSource(CancellationToken.None);
        }


        private void ViewSource_DataChanged(object? sender, DataSourceChangedEventArgs e)
        {
            // NOTE: Unload()等でデータがないときは更新しない
            if (e.DataSource.DataState == DataState.None) return;

            AppDispatcher.BeginInvoke(() => UpdateContent(e.DataSource));
        }


        // コントロールサイズの更新
        protected bool UpdateSize()
        {
            bool sizeChanged = (Width != LayoutSize.Width || Height != LayoutSize.Height);

            _sizeSource.Width = LayoutSize.Width;
            _sizeSource.Height = LayoutSize.Height;

            return sizeChanged;
        }

        /// <summary>
        /// ViewSource の更新
        /// </summary>
        /// <param name="token"></param>
        protected void RequestLoadViewSource(CancellationToken token)
        {
            if (!_element.Page.Content.IsLoaded) return;
            Task.Run(() => LoadViewSourceAsync(token));
        }

        /// <summary>
        /// Load ViewSource
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadViewSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            var pictureSize = _viewContentSize.GetPictureSize();
            //Debug.WriteLine($"ViewContent.LoadViewSourceAsync: {_element.Page}, PictureSize = {pictureSize:f0}");
            await _viewSource.LoadAsync(pictureSize, token);
        }

        /// <summary>
        /// Update ViewContent
        /// </summary>
        protected void UpdateContent(DataSource data)
        {
            NVDebug.AssertSTA();
            var unit = CreateContent(_sizeSource, data);
            Content = unit.Content;
            State = unit.State;
            UpdateSize();
            ViewContentChanged?.Invoke(this, new ViewContentChangedEventArgs(State.ToChangedAction(), this));
        }



        protected virtual ViewContentData CreateContent(SizeSource size, DataSource data)
        {
            Debug.Assert(_initialized);

            switch (data.DataState)
            {
                case DataState.None:
                    Debug.WriteLine($"CreateContent.Ready: {ArchiveEntry}");
                    return new ViewContentData(ViewContentTools.CreateLoadingContent(Element), ViewContentState.Loading);

                case DataState.Loaded:
                    Debug.WriteLine($"CreateContent.Loaded: {ArchiveEntry}");
                    Debug.Assert(data.Data is not null);
                    return new ViewContentData(new DecoratedViewContent(size, CreateLoadedContent(data.Data), _backgroundSource), ViewContentState.Loaded);

                case DataState.Failed:
                    Debug.WriteLine($"CreateContent.Failed: {ArchiveEntry}");
                    return new ViewContentData(ViewContentTools.CreateErrorContent(Element, data.ErrorMessage), ViewContentState.Failed);

                default:
                    throw new InvalidOperationException();
            }
        }

        protected abstract FrameworkElement CreateLoadedContent(object data);
    }




    public class DecoratedViewContent : Grid, IDisposable
    {
        private FrameworkElement _content;
        private GridLine _gridLine;
        private bool _disposedValue;


        public DecoratedViewContent(SizeSource sizeSource, FrameworkElement content, PageBackgroundSource backgroundSource)
        {
            // background
            var rectangle = new Rectangle();
            rectangle.SetBinding(Rectangle.FillProperty, new Binding(nameof(PageBackgroundSource.Brush)) { Source = backgroundSource });
            rectangle.Margin = new Thickness(1);
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;
            this.Children.Add(rectangle);

            // content
            _content = content;
            sizeSource.BindTo(_content);
            this.Children.Add(_content);

            // grid line
            _gridLine = new GridLine();
            sizeSource.BindTo(_gridLine);
            this.Children.Add(_gridLine);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _gridLine.Dispose();
                    (_content as IDisposable)?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



    public record class ViewContentData(FrameworkElement Content, ViewContentState State);



    public enum ViewContentState
    {
        None,
        Loading,
        Loaded,
        Failed,
    }

    public static class ViewContentStateExtensions
    {
        public static ViewContentChangedAction ToChangedAction(this ViewContentState state)
        {
            return state switch
            {
                ViewContentState.None => ViewContentChangedAction.ContentLoading,
                ViewContentState.Loading => ViewContentChangedAction.ContentLoading,
                ViewContentState.Loaded => ViewContentChangedAction.ContentLoaded,
                ViewContentState.Failed => ViewContentChangedAction.ContentFailed,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}
