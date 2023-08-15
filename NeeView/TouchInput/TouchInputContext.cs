using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    //
    public class TouchInputContext
    {
        public TouchInputContext(FrameworkElement sender, FrameworkElement? target, MouseGestureCommandCollection? gestureCommandCollection, DragTransform? dragTransform, IDragTransformControl? dragTransformControl)
        {
            this.Sender = sender;
            this.Target = target;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransform = dragTransform;
            this.DragTransformControl = dragTransformControl;
            //this.LoupeTransform = loupeTransform;
        }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement? Target { get; set; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; set; }

        /// <summary>
        /// 操作する座標系
        /// </summary>
        public DragTransform? DragTransform { get; set; }

        public IDragTransformControl? DragTransformControl { get; set; }

        //public LoupeTransform? LoupeTransform { get; set; }

        /// <summary>
        /// 有効なタッチデバイス情報
        /// </summary>
        public Dictionary<StylusDevice, TouchContext> TouchMap { get; set; } = new Dictionary<StylusDevice, TouchContext>();

        /// <summary>
        /// 表示コンテンツのエリア情報取得
        /// </summary>
        public DragArea GetArea()
        {
            // TargetをnullにしたときはGetArea()を使用する処理にはならないはず
            Debug.Assert(this.Target is not null);

#warning not support yet
            var viewRect = new Rect(0, 0, this.Sender.ActualWidth, this.Sender.ActualHeight);
            return new DragArea(viewRect, viewRect); // ##
        }

        /// <summary>
        /// 表示コンテンツのエリア情報取得
        /// </summary>
        /// <returns></returns>
        //public DragAreaX GetArea()
        //{
        //    // TargetをnullにしたときはGetArea()を使用する処理にはならないはず
        //    Debug.Assert(this.Target is not null);

        //    return new DragAreaX(this.Sender, this.Target ?? this.Sender);
        //}
    }
}
