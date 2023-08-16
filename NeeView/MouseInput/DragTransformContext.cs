using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using NeeView.ComponentModel;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// 表示座標系操作のリソース
    /// </summary>
    public class DragTransformContext 
    {
        private int _speedLatestTimestamp;
        private Point _speedLatestPoint;

        public DragTransformContext(FrameworkElement sender, ITransformControl transform, Rect contentRect, Rect viewRect, ViewConfig viewConfig)
        {
            Sender = sender;
            ViewRect = viewRect;
            ViewConfig = viewConfig;
            ContentRect = contentRect;
            Transform = transform;
        }

        public ViewConfig ViewConfig { get; }

        public ITransformControl Transform { get; set; }

        public FrameworkElement Sender { get; }

        // NOTE: 画面中央を(0,0)とした座標系
        public Point OriginPoint { get; set; }
        public Point First { get; set; }
        public Point Old { get; set; }
        public Point Last { get; set; }

        public int FirstTimeStamp { get; set; }
        public int OldTimeStamp { get; set; }
        public int LastTimeStamp { get; set; }

        public Rect ViewRect { get; set; }
        public Rect ContentRect { get; set; }

        public Point ContentCenter => ContentRect.Center();

        public Point BasePoint { get; set; }
        public double BaseAngle { get; set; }
        public double BaseScale { get; set; }
        public bool BaseFlipHorizontal { get; set; }
        public bool BaseFlipVertical { get; set; }

        public Point RotateCenter { get; set; }
        public Point ScaleCenter { get; set; }
        public Point FlipCenter { get; set; }

        public Vector Speed { get; set; } // dot/ms



        public virtual void Initialize(Point point, int timestamp)
        {
            OriginPoint = point;

            First = point;
            Old = point;
            Last = point;

            FirstTimeStamp = timestamp;
            OldTimeStamp = timestamp;
            LastTimeStamp = timestamp;

            BasePoint = Transform.Point;
            BaseAngle = Transform.Angle;
            BaseScale = Transform.Scale;
            BaseFlipHorizontal = Transform.IsFlipHorizontal;
            BaseFlipVertical = Transform.IsFlipVertical;

            Speed = default;
            _speedLatestPoint = Last;
            _speedLatestTimestamp = LastTimeStamp;

            RotateCenter = GetCenterPosition(ViewConfig.RotateCenter);
            ScaleCenter = GetCenterPosition(ViewConfig.ScaleCenter);
            FlipCenter = GetCenterPosition(ViewConfig.FlipCenter);
        }


        private Point GetCenterPosition(DragControlCenter dragControlCenter)
        {
            return dragControlCenter switch
            {
                DragControlCenter.View => ViewRect.Center(),
                DragControlCenter.Target => ContentRect.Center(),
                DragControlCenter.Cursor => First,
                _ => throw new NotImplementedException(),
            };
        }


        public void Update(Point point, int timestamp)
        {
            Old = Last;
            OldTimeStamp = LastTimeStamp;
            Last = point;
            LastTimeStamp = timestamp;
        }


        public void UpdateSpeed(Point point, int timestamp)
        {
            var time = timestamp - _speedLatestTimestamp;
            if (time <= 0) return;

            var delta = point - _speedLatestPoint;
            var speed = delta / time;
            Speed = (Speed + speed * time) / (1 + time);
            _speedLatestPoint = point;
            _speedLatestTimestamp = timestamp;

            Debug.Assert(!double.IsNaN(Speed.X));
            Debug.Assert(!double.IsNaN(Speed.Y));
        }
    }



}