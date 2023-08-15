using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MouseInputContext : BindableBase
    {
        public MouseInputContext(FrameworkElement sender, MouseGestureCommandCollection? gestureCommandCollection, IDragTransformControl? dragTransformControl, DragTransform? dragTransform)
        {
            this.Sender = sender;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransformControl = dragTransformControl;
            this.DragTransform = dragTransform;
            //this.LoupeTransform = loupeTransform;
        }


        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; init; }
        
        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; init; }

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
