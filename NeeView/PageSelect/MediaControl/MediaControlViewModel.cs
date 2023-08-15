using NeeLaboratory.ComponentModel;
using System;
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
        }

        public MediaPlayerOperator? Operator
        {
            get { return _operator; }
            set { if (_operator != value) { _operator = value; RaisePropertyChanged(); } }
        }

        #region Methods

        private void Model_Changed(object? sender, MediaPlayerChanged e)
        {
            Operator?.Dispose();

            if (e.IsValid)
            {
                var mediaPlayse = e.MediaPlayer ?? throw new InvalidOperationException();
                var uri = e.Uri ?? throw new InvalidOperationException();
                Operator = new MediaPlayerOperator(mediaPlayse);
                Operator.MediaEnded += Operator_MediaEnded;
                Operator.Open(uri, e.IsLastStart);
            }
            else
            {
                Operator = null;
            }

            MediaPlayerOperator.Current = Operator;
        }

        private void Operator_MediaEnded(object? sender, System.EventArgs e)
        {
#warning not implement yet
            //BookOperation.Current.Book?.Viewer.RaisePageTerminatedEvent(this, 1);
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
