using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public interface IMediaPlayer : INotifyPropertyChanged
    {
        bool HasAudio { get; }
        bool HasVideo { get; }
        bool IsEnabled { get; set; }
        bool IsMuted { get; set; }
        bool IsRepeat { get; set; }
        bool IsPlaying { get; }
        Duration NaturalDuration { get; }
        TimeSpan Position { get; set; }
        bool ScrubbingEnabled { get; set; }
        double Volume { get; set; }

        event EventHandler MediaEnded;
        event EventHandler<ExceptionEventArgs> MediaFailed;
        event EventHandler MediaOpened;
        event EventHandler? MediaPlayed;


        void Play();
        void Pause();

        IDisposable SubscribePropertyChanged(string propertyName, PropertyChangedEventHandler handler);
    }
}