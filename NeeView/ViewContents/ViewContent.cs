using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView;
using System.Windows.Input;
using System.Windows.Threading;
using NeeView.Windows;
using NeeLaboratory.Generators;

namespace NeeView
{
    public abstract partial class ViewContent : ContentControl, IDisposable
    {
        private bool _initialized;
        private PageFrameElement _element;
        private PageFrameElementScale _scale;
        //private readonly ViewContentParameters? _parameter;
        private PageFrameActivity _activity;
        private ViewSource _viewSource;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        private ViewContentSize _viewContentSize;


        public ViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
        {
            _element = element;
            _scale = scale;
            //_parameter = parameter;
            _viewSource = viewSource;
            _activity = activity;

            _viewContentSize = ViewContentSizeFactory.Create(element, scale);

            Width = LayoutSize.Width;
            Height = LayoutSize.Height;
        }


        [Subscribable]
        public event EventHandler? ViewContentChanged;



        public PageFrameActivity Activity => _activity;


        public ArchiveEntry ArchiveEntry => _element.Page.ArchiveEntry;
        public PageFrameElement Element => _element;

        //[Obsolete]
        //public ViewContentParameters? Parameter => _parameter;

        public ViewContentSize ViewContentSize => _viewContentSize;
        public Size LayoutSize => _viewContentSize.LayoutSize;

        public ViewSource ViewSource => _viewSource;



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

            _disposables.Add(_viewSource.SubscribeDataSourceChanged(ViewSouorce_DataChanged));
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

            Debug.WriteLine($"OnSourceChanged: {ArchiveEntry} ({LayoutSize})");
            OnSourceChanged();
        }

        protected virtual void OnSourceChanged()
        {
            UpdateSize();
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


        private void ViewSouorce_DataChanged(object? sender, DataSourceChangedEventArgs e)
        {
            // NOTE: Unload()等でデータがないときは更新しない
            if (e.DataSource.DataState == DataState.None) return;

            AppDispatcher.BeginInvoke(() => UpdateContent(e.DataSource));
        }


        // コントロールサイズの更新
        protected void UpdateSize()
        {
            Width = LayoutSize.Width;
            Height = LayoutSize.Height;

            if (this.Content is not FrameworkElement control) return;
            control.Width = LayoutSize.Width;
            control.Height = LayoutSize.Height;
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
            (Content as IDisposable)?.Dispose();
            Content = CreateContent(LayoutSize, data);
            UpdateSize();
            ViewContentChanged?.Invoke(this, EventArgs.Empty);
        }


        protected virtual FrameworkElement CreateContent(Size size, DataSource data)
        {
            Debug.Assert(_initialized);

            switch (data.DataState)
            {
                case DataState.None:
                    Debug.WriteLine($"CreateContent.Ready: {ArchiveEntry}");
                    return ViewContentTools.CreateLoadingContent(Element);

                case DataState.Loaded:
                    Debug.WriteLine($"CreateContent.Loaded: {ArchiveEntry}");
                    Debug.Assert(data.Data is not null);
                    return CreateLoadedContent(size, data.Data);

                case DataState.Failed:
                    Debug.WriteLine($"CreateContent.Failed: {ArchiveEntry}");
                    return ViewContentTools.CreateErrorContent(Element, data.ErrorMessage);

                default:
                    throw new InvalidOperationException();
            }
        }


        protected virtual FrameworkElement CreateLoadedContent(Size size, object data)
        {
            return ViewContentTools.CreateDummyContent(Element);
        }
    }
}
