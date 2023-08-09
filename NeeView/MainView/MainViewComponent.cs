using NeeLaboratory.ComponentModel;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MainViewComponent : IDisposable
    {
        private static MainViewComponent? _current;
        public static MainViewComponent Current => _current ?? throw new InvalidOperationException();


        private readonly MainView _mainView;
        private TouchEmurlateController _touchEmurlateController = new();
        private bool _disposedValue;


        public static void Initlialize()
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = new MainViewComponent();
        }

        // TODO: MainView依存はおかしい
        // TODO: 各種シングルトン依存の排除
        private MainViewComponent()
        {
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            var bookHub = BookHub.Current;

            _mainView = new MainView();

            DragTransform = new DragTransform();
            DragTransformControl = new DragTransformControl(DragTransform, _mainView.View, _mainView.MainContentShadow);
            LoupeTransform = new LoupeTransform();

            TouchInput = new TouchInput(new TouchInputContext(_mainView.View, _mainView.MainContentShadow, mouseGestureCommandCollection, DragTransform, DragTransformControl, LoupeTransform));
            MouseInput = new MouseInput(new MouseInputContext(_mainView.View, mouseGestureCommandCollection, DragTransformControl, DragTransform, LoupeTransform));

            var scrollPageController = new ScrollPageController(this, BookSettingPresenter.Current, BookOperation.Current);
            PrintController = new PrintController(this, _mainView);
            ViewTransformControl = new ViewTransformControl(this, scrollPageController);
            ViewLoupeControl = new ViewLoupeControl(this);
            ViewWindowControl = new ViewWindowControl(this);
            ViewPropertyControl = new ViewPropertyControl(this);
            ViewCopyImage = new ViewCopyImage(this);

            ContentCanvas = new ContentCanvas(this, bookHub);
            ContentCanvasBrush = new ContentCanvasBrush(ContentCanvas);

            ContentRebuild = new ContentRebuild(this);

            _mainView.DataContext = new MainViewViewModel(this);
        }


        /// <summary>
        /// コンテキストメニューを開く要求イベント
        /// </summary>
        public event EventHandler? OpenContextMenuRequest;

        public IDisposable SubscribeOpenContextMenuRequest(EventHandler handler)
        {
            OpenContextMenuRequest += handler;
            return new AnonymousDisposable(() => OpenContextMenuRequest -= handler);
        }

        /// <summary>
        /// MainViewにフォーカスを移す要求イベント
        /// </summary>
        public event EventHandler? FocusMainViewRequest;

        public IDisposable SubscribeFocusMainViewRequest(EventHandler handler)
        {
            FocusMainViewRequest += handler;
            return new AnonymousDisposable(() => FocusMainViewRequest -= handler);
        }


        public MainView MainView => _mainView;

        public DragTransform DragTransform { get; private set; }
        public DragTransformControl DragTransformControl { get; private set; }
        public LoupeTransform LoupeTransform { get; private set; }

        public MouseInput MouseInput { get; private set; }
        public TouchInput TouchInput { get; private set; }

        public PrintController PrintController { get; private set; }

        public IViewTransformControl ViewTransformControl { get; private set; }
        public IViewLoupeControl ViewLoupeControl { get; private set; }
        public IViewWindowControl ViewWindowControl { get; private set; }
        public IViewPropertyControl ViewPropertyControl { get; private set; }
        public IViewCopyImage ViewCopyImage { get; private set; }

        public ContentCanvas ContentCanvas { get; private set; }
        public ContentCanvasBrush ContentCanvasBrush { get; private set; }

        public ContentRebuild ContentRebuild { get; private set; }


        public bool IsLoupeMode => ViewLoupeControl.GetLoupeMode();


        public Window GetWindow()
        {
            return Window.GetWindow(_mainView);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ContentCanvas.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void RaiseOpenContextMenuRequest()
        {
            OpenContextMenuRequest?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseFocusMainViewRequest()
        {
            FocusMainViewRequest?.Invoke(this, EventArgs.Empty);
        }

        public void TouchInputEmutrate(object? sender)
        {
            _touchEmurlateController.Execute(sender);
        }
    }
}
