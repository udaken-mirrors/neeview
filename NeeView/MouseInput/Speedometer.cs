using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ポインタ移動の速度計測
    /// </summary>
    public class Speedometer
    {
        private bool _initialized;
        private int _timestamp;
        private Point _point;
        private Vector _speed;

        /// <summary>
        /// 速度 (dot/ms)
        /// </summary>
        public Vector Speed => _speed;

        public void Initialize(Point point, int timestamp)
        {
            _point = point;
            _timestamp = timestamp;
            _speed = new Vector();
            _initialized = true;
        }

        public void Update(Point point, int timestamp)
        {
            if (!_initialized)
            {
                Initialize(point, timestamp);
                return;
            }

            var span = timestamp - _timestamp;
            if (span <= 0) return;

            var delta = point - _point;
            var speed = delta / span;
            // これまでの speed を500ms程度の重みで計算
            var sourceSpan = Math.Max(500 - span, 1);
            _speed = (_speed * sourceSpan + speed * span) / (sourceSpan + span);

            _point = point;
            _timestamp = timestamp;
            
            //Debug.WriteLine($">> {time}: {delta:f0} -> {Speed:f1}");

            Debug.Assert(!double.IsNaN(_speed.X));
            Debug.Assert(!double.IsNaN(_speed.Y));
        }
    }
}