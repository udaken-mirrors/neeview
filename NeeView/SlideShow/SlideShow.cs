using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

// TODO: Timerをもっと制度の良いものに変更

namespace NeeView
{
    /// <summary>
    /// スライドショー管理
    /// </summary>
    public class SlideShow : BindableBase, IDisposable
    {
        static SlideShow() => Current = new SlideShow();
        public static SlideShow Current { get; }

        // タイマーディスパッチ
        private readonly DispatcherTimer _timer;

        // スライドショー表示間隔用
        private DateTime _lastShowTime;

        private const double _minTimerTick = 0.01;
        private const double _maxTimerTick = 0.2;

        private bool _isPlayingSlideShow;
        private bool _isPlayingSlideShowMemento;
        private readonly DisposableCollection _disposables = new();


        // コンストラクター
        private SlideShow()
        {
            // timer for slideshow
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_maxTimerTick);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);

            _disposables.Add(BookOperation.Current.SubscribeViewContentsChanged(
                (s, e) => ResetTimer()));

            _disposables.Add(MainWindow.Current.SubscribePreviewKeyDown(
                (s, e) => ResetTimer()));

            _disposables.Add(MainViewComponent.Current.MainView.SubscribePreviewKeyDown(
                (s, e) => ResetTimer()));

            _disposables.Add(MainViewComponent.Current.MouseInput.SubscribeMouseMoved(
                (s, e) =>
                {
                    if (Config.Current.SlideShow.IsCancelSlideByMouseMove)
                    {
                        ResetTimer();
                    }
                }));

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        /// <summary>
        /// スライドショー再生状態
        /// </summary>
        public bool IsPlayingSlideShow
        {
            get { return _isPlayingSlideShow; }
            set
            {
                if (_disposedValue) return;

                if (_isPlayingSlideShow != value)
                {
                    _isPlayingSlideShow = value;
                    if (_isPlayingSlideShow)
                    {
                        StartTimer();
                    }
                    else
                    {
                        StopTimer();
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public void Play()
        {
            IsPlayingSlideShow = true;
        }

        public void Stop()
        {
            IsPlayingSlideShow = false;
        }

        /// <summary>
        /// スライドショー再生/停止切り替え
        /// </summary>
        public void TogglePlayingSlideShow()
        {
            if (_disposedValue) return;

            this.IsPlayingSlideShow = !this.IsPlayingSlideShow;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void PauseSlideShow()
        {
            if (_disposedValue) return;

            _isPlayingSlideShowMemento = IsPlayingSlideShow;
            IsPlayingSlideShow = false;
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void ResumeSlideShow()
        {
            if (_disposedValue) return;

            IsPlayingSlideShow = _isPlayingSlideShowMemento;
        }

        /// <summary>
        /// 次のスライドへ移動：スライドショー専用
        /// </summary>
        private void NextSlide()
        {
            BookOperation.Current.NextSlide(this);
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void StartTimer()
        {
            if (_disposedValue) return;

            // インターバル時間を修正する
            _timer.Interval = TimeSpan.FromSeconds(MathUtility.Clamp(Config.Current.SlideShow.SlideShowInterval * 0.5, _minTimerTick, _maxTimerTick));
            _lastShowTime = DateTime.Now;
            _timer.Start();
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        private void StopTimer()
        {
            _timer.Stop();
        }

        /// <summary>
        /// スライドショータイマーリセット
        /// </summary>
        public void ResetTimer()
        {
            if (_disposedValue) return;

            if (!_timer.IsEnabled) return;
            _lastShowTime = DateTime.Now;
        }

        /// <summary>
        /// 再生中のタイマー処理
        /// </summary>
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            // マウスボタンが押されていたらキャンセル
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _lastShowTime = DateTime.Now;
                return;
            }

            // スライドショーのインターバルを非アクティブ時間で求める
            if ((DateTime.Now - _lastShowTime).TotalSeconds >= Config.Current.SlideShow.SlideShowInterval)
            {
                if (!BookHub.Current.IsLoading) NextSlide();
                _lastShowTime = DateTime.Now;
            }
        }

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
                    _timer.Stop();
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
            public double SlideShowInterval { get; set; }

            [DataMember]
            public bool IsCancelSlideByMouseMove { get; set; }

            [DataMember]
            public bool IsSlideShowByLoop { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsAutoPlaySlideShow { get; set; }


            public void RestoreConfig(Config config)
            {
                config.SlideShow.SlideShowInterval = SlideShowInterval;
                config.SlideShow.IsCancelSlideByMouseMove = IsCancelSlideByMouseMove;
                config.SlideShow.IsSlideShowByLoop = IsSlideShowByLoop;
                config.StartUp.IsAutoPlaySlideShow = IsAutoPlaySlideShow;
            }
        }

        #endregion

    }
}
