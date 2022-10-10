using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{
    public class MediaControl : BindableBase
    {
        static MediaControl() => Current = new MediaControl();
        public static MediaControl Current { get; }


        private MediaControl()
        {
        }

        public event EventHandler<MediaPlayerChanged>? Changed;


        public void RiaseContentChanged(object sender, MediaPlayerChanged e)
        {
            Changed?.Invoke(sender, e);
        }

    }

    /// <summary>
    /// MediaPlayer変更通知パラメータ
    /// </summary>
    public class MediaPlayerChanged : EventArgs
    {
        public MediaPlayerChanged()
        {
        }

        public MediaPlayerChanged(MediaPlayer player, Uri uri, bool isLastStart)
        {
            MediaPlayer = player;
            Uri = uri;
            IsLastStart = isLastStart;
        }

        public MediaPlayer? MediaPlayer { get; set; }
        public Uri? Uri { get; set; }
        public bool IsLastStart { get; set; }
        public bool IsValid => MediaPlayer != null;
    }
}
