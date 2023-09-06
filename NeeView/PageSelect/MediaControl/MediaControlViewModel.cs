using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    public class MediaControlViewModel : BindableBase
    {
        private readonly MediaControl _model;
        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private MediaPlayerOperator? _operator;

        public MediaControlViewModel(MediaControl model)
        {
            _model = model;
            _model.Changed += Model_Changed;

            PlayCommand = new RelayCommand(PlayCommand_Executed);
            RepeatCommand = new RelayCommand(RepeatCommand_Executed);
            MuteCommand = new RelayCommand(MuteCommand_Executed);
        }

        public MediaPlayerOperator? Operator
        {
            get { return _operator; }
            set
            {
                if (_operator != value)
                {
                    AttachOperator(value);
                    //_operator = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DisposableCollection _operatorEventDisposables = new();

        private void AttachOperator(MediaPlayerOperator? op)
        {
            DetachOperator();
            if (op is null) return;

            _operator = op;

            _operatorEventDisposables = new();

            _operatorEventDisposables.Add(_operator.SubscribePropertyChanged(nameof(_operator.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));
        }

        private void DetachOperator()
        {
            if (_operator is null) return;

            _operatorEventDisposables.Dispose();
            _operator = null;
        }



        public bool IsPlaying => _operator?.IsPlaying ?? false;


        public RelayCommand PlayCommand { get; }
        public RelayCommand RepeatCommand { get; }
        public RelayCommand MuteCommand { get; }


        private void PlayCommand_Executed()
        {
            if (Operator is null) return;
            Operator.TogglePlay();
        }

        private void RepeatCommand_Executed()
        {
            if (Operator is null) return;
            Operator.IsRepeat = !Operator.IsRepeat;
        }

        private void MuteCommand_Executed()
        {
            if (Operator is null) return;
            Operator.IsMuted = !Operator.IsMuted;
        }




        #region Methods

        private void Model_Changed(object? sender, MediaPlayerChanged e)
        {
            Operator?.Dispose();

            if (e.IsValid)
            {
                var mediaPlayer = e.MediaPlayer ?? throw new InvalidOperationException();
                Operator = new MediaPlayerOperator(mediaPlayer);
                Operator.MediaEnded += Operator_MediaEnded;
                Operator.Attach(true);
#if false
                var mediaPlayer = e.MediaPlayer ?? throw new InvalidOperationException();
                var uri = e.Uri ?? throw new InvalidOperationException();
                Operator = new MediaPlayerOperator(mediaPlayer);
                Operator.MediaEnded += Operator_MediaEnded;
                Operator.Open(uri, e.IsLastStart);
#endif
            }
            else
            {
                Operator = null;
            }

            RaisePropertyChanged("");


            if (e.IsMainMediaPlayer)
            {
                MediaPlayerOperator.Current = Operator;
            }
        }

        private void Operator_MediaEnded(object? sender, System.EventArgs e)
        {
            PageFrameBoxPresenter.Current.View?.RaisePageTerminatedEvent(this, 1);
        }

        public void SetScrubbing(bool isScrubbing)
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsScrubbing = isScrubbing;
        }

        public void ToggleTimeFormat()
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsTimeLeftDisp = !_operator.IsTimeLeftDisp;
        }

        public void MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn == 0) return;

            for (int i = 0; i < Math.Abs(turn); ++i)
            {
                if (turn < 0)
                {
                    BookOperation.Current.Control.MoveNext(this);
                }
                else
                {
                    BookOperation.Current.Control.MovePrev(this);
                }
            }
        }

        internal void MouseWheelVolume(object? sender, MouseWheelEventArgs e)
        {
            if (Operator is null) return;

            var delta = (double)e.Delta / 6000.0;
            Operator.AddVolume(delta);
        }

        internal bool KeyVolume(Key key)
        {
            if (Operator is null) return false;

            switch (key)
            {
                case Key.Up:
                case Key.Right:
                    Operator.AddVolume(+0.01);
                    return true;

                case Key.Down:
                case Key.Left:
                    Operator.AddVolume(-0.01);
                    return true;

                default:
                    return false;
            }
        }

        #endregion
    }
}
