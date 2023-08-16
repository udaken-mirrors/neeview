﻿using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MouseInputContext : BindableBase
    {

        public MouseInputContext(FrameworkElement sender, MouseGestureCommandCollection? gestureCommandCollection, IDragTransformContextFactory? dragTransformContextFactory, IDragTransformControl? dragTransformControl, DragTransform? dragTransform, LoupeContext? loupe)
        {
            this.Sender = sender;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransformContextFactory = dragTransformContextFactory;
            this.DragTransformControl = dragTransformControl;
            this.DragTransform = dragTransform;
            //this.LoupeTransform = loupeTransform;
            this.Loupe = loupe;
        }


        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; init; }

        public LoupeContext? Loupe { get; init; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; init; }

        public IDragTransformContextFactory? DragTransformContextFactory { get; init; }

        public IDragTransformControl? DragTransformControl { get; init; }

        public DragTransform? DragTransform { get; init; }

        //public LoupeTransform? LoupeTransform { get; init; }

        public bool IsGestureEnabled { get; init; } = true;

        public bool IsLeftButtonDownEnabled { get; init; } = true;

        public bool IsRightButtonDownEnabled { get; init; } = true;

        public bool IsVerticalWheelEnabled { get; init; } = true;

        public bool IsHorizontalWheelEnabled { get; init; } = true;

        public bool IsMouseEventTerminated { get; init; } = true;

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        public int StartTimestamp { get; set; }
    }

}
