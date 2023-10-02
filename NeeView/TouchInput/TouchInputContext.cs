using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class TouchInputContext
    {
        public TouchInputContext(FrameworkElement sender, MouseGestureCommandCollection? gestureCommandCollection, IDragTransformContextFactory? dragTransformContextFactory, IDragTransformControl? dragTransformControl, LoupeContext? loupe)
        {
            this.Sender = sender;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransformContextFactory = dragTransformContextFactory;
            this.DragTransformControl = dragTransformControl;
            this.Loupe = loupe;

        }


        public LoupeContext? Loupe { get; init; }


        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; set; }

        public IDragTransformContextFactory? DragTransformContextFactory { get; init; }

        /// <summary>
        /// 操作する座標系
        /// </summary>
        public IDragTransformControl? DragTransformControl { get; set; }


        /// <summary>
        /// 有効なタッチデバイス情報
        /// </summary>
        public Dictionary<StylusDevice, TouchContext> TouchMap { get; set; } = new Dictionary<StylusDevice, TouchContext>();

        /// <summary>
        /// 速度計測器
        /// </summary>
        public MultiSpeedometer Speedometer { get; } = new MultiSpeedometer();
    }

}
