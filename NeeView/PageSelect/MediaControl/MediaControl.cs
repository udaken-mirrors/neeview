﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{


    public class MediaControl : BindableBase
    {
        public MediaControl()
        {
        }

        public event EventHandler<MediaPlayerChanged>? Changed;

        public MediaPlayerChanged LastChangedArgs { get; private set; } = new();


        public void RaiseContentChanged(object sender, MediaPlayerChanged e)
        {
            LastChangedArgs = e;
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

        public MediaPlayerChanged(IMediaPlayer player, bool isLastStart)
        {
            MediaPlayer = player;
            //Uri = uri;
            IsLastStart = isLastStart;
        }

        public IMediaPlayer? MediaPlayer { get; set; }
        //public Uri? Uri { get; set; }
        public bool IsLastStart { get; set; }
        public bool IsValid => MediaPlayer != null;
        public bool IsMainMediaPlayer { get; init; }
    }
}
