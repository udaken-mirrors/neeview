using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class MediaControlViewModel : BindableBase
    {
        private readonly MediaControl _model;
        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private MediaPlayerOperator? _operator;
        private DisposableCollection _operatorEventDisposables = new();
        private bool _isMoreMenuEnabled;


        public MediaControlViewModel(MediaControl model)
        {
            _model = model;
            _model.Changed += Model_Changed;

            PlayCommand = new RelayCommand(PlayCommand_Executed);
            RepeatCommand = new RelayCommand(RepeatCommand_Executed);
            MuteCommand = new RelayCommand(MuteCommand_Executed);

            MoreMenuDescription = new MediaPlayerMoreMenuDescription(this);
        }


        public MediaPlayerOperator? Operator
        {
            get { return _operator; }
            set
            {
                if (_operator != value)
                {
                    AttachOperator(value);
                    RaisePropertyChanged();
                    IsMoreMenuEnabled = _operator?.CanControlTracks == true;
                }
            }
        }

        public bool IsMoreMenuEnabled
        {
            get { return _isMoreMenuEnabled; }
            set { SetProperty(ref _isMoreMenuEnabled, value); }
        }
      
        public bool IsPlaying => _operator?.IsPlaying ?? false;


        public RelayCommand PlayCommand { get; }
        public RelayCommand RepeatCommand { get; }
        public RelayCommand MuteCommand { get; }


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
            _operatorEventDisposables.Clear();
            _operator = null;
        }

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


        private void Model_Changed(object? sender, MediaPlayerChanged e)
        {
            Operator?.Dispose();

            if (e.IsValid)
            {
                var mediaPlayer = e.MediaPlayer ?? throw new InvalidOperationException();
                Operator = new MediaPlayerOperator(mediaPlayer);
                Operator.MediaEnded += Operator_MediaEnded;
                Operator.Attach();
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
            PageFrameBoxPresenter.Current.View?.RaisePageTerminatedEvent(this, 1, true);
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


        #region MoreMenu

        public MediaPlayerMoreMenuDescription MoreMenuDescription { get; }

        public class MediaPlayerMoreMenuDescription : MoreMenuDescription
        {
            private readonly MediaControlViewModel _vm;
            private readonly ContextMenu _menu = new();
            private readonly MatchingToBooleanConverter<TrackItem> _matchingConverter = new ();

            public MediaPlayerMoreMenuDescription(MediaControlViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Update(ContextMenu menu)
            {
                return Create();
            }

            public override ContextMenu Create()
            {
                var menu = _menu;
                _menu.Items.Clear();

                if (_vm.Operator is null) return menu;

                var audios = _vm.Operator.AudioTracks;
                if (audios is not null)
                {
                    foreach (var track in audios.Tracks)
                    {
                        menu.Items.Add(CreateTrackMenuItem(track, audios));
                    }
                }
                else
                {
                    menu.Items.Add(new MenuItem() { Header = Properties.Resources.MediaControl_MoreMenu_NoAudio, IsEnabled = false });
                }

                menu.Items.Add(new Separator());

                var subtitles = _vm.Operator.SubtitleTracks;
                if (subtitles is not null)
                {
                    foreach (var track in subtitles.Tracks)
                    {
                        menu.Items.Add(CreateTrackMenuItem(track, subtitles));
                    }
                }
                else
                {
                    menu.Items.Add(new MenuItem() { Header = Properties.Resources.MediaControl_MoreMenu_NoSubtitle, IsEnabled = false });
                }

                return menu;
            }

            private MenuItem CreateTrackMenuItem(TrackItem track, TrackCollection tracks)
            {
                var menuItem = new MenuItem()
                {
                    Header = track.Name,
                };
                menuItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(tracks.Selected))
                {
                    Source = tracks,
                    Converter = _matchingConverter,
                    ConverterParameter = track,
                });

                menuItem.Click += (s, e) => { tracks.Selected = track; };

                return menuItem;
            }
        }

        #endregion MoreMenu

    }



    public class MatchingToBooleanConverter<T> : IValueConverter
        where T : class
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedItem = value as T;
            var selfItem = parameter as T;

            var result = selectedItem is not null && selectedItem == selfItem;
            return result;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
