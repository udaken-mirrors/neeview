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
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// スライドショー管理
    /// </summary>
    public class SlideShow : BindableBase, IDisposable
    {
        static SlideShow() => Current = new SlideShow();
        public static SlideShow Current { get; }

        private readonly Timer _timer;

        private bool _isPlayingSlideShow;
        private bool _isPlayingSlideShowMemento;
        private readonly DisposableCollection _disposables = new();


        private SlideShow()
        {
            // timer for slideshow
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Tick;

            _disposables.Add(Config.Current.SlideShow.SubscribePropertyChanged(nameof(SlideShowConfig.SlideShowInterval),
                (s, e) => UpdateTimerInterval()));

            _disposables.Add(BookOperation.Current.SubscribeBookChanged(
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

        private void UpdateTimerInterval()
        {
            if (_disposedValue) return;

            _timer.Interval = Math.Max(Config.Current.SlideShow.SlideShowInterval * 1000.0, 1.0);
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void StartTimer()
        {
            if (_disposedValue) return;

            UpdateTimerInterval();
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
            if (!_timer.Enabled) return;

            UpdateTimerInterval();
        }

        /// <summary>
        /// 再生中のタイマー処理
        /// </summary>
        private void Timer_Tick(object? sender, ElapsedEventArgs e)
        {
            if (_disposedValue) return;

            AppDispatcher.BeginInvoke(() =>
            {
                // ドラッグ中はキャンセル
                if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
                {
                    ResetTimer();
                    return;
                }

                // ブック有効ならばページ移動
                if (BookOperation.Current.IsEnabled)
                {
                    NextSlide();
                }
            });
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
