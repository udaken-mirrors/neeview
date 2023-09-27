//#define LOCAL_DEBUG

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ポインタ移動の速度計測
    /// </summary>
    public class Speedometer
    {
        private readonly RingList<PointRecord> _points;

        public Speedometer()
        {
            _points = new RingList<PointRecord>(3);
        }

        public void Reset()
        {
            _points.Clear();
        }

        public void Reset(Point point, int timestamp)
        {
            _points.Clear();
            Update(point, timestamp);
        }

        public void Update(Point point, int timestamp)
        {
            if (_points.Any(e => e.Timestamp == timestamp)) return;

#if LOCAL_DEBUG
            if (_points.Any())
            {
                var a = _points.LastOrDefault();
                Debug.WriteLine($">> Span: {timestamp} ({timestamp - a.Timestamp})");
            }
#endif

            _points.Add(new PointRecord(point, timestamp));
        }

        public Vector GetSpeed()
        {
#if LOCAL_DEBUG
            for (int i = 0; i < _points.Count; i++)
            {
                var p = _points[i];
                Trace($"# {i}: Point = {p.Point:f2}, Timestamp = {p.Timestamp}");
            }
#endif

            var points = _points.OrderByDescending(e => e.Timestamp).ToList();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                Trace($"{i}: Point = {p.Point:f2}, Timestamp = {p.Timestamp}");
            }

            int totalSpan = 0;
            Vector speedSum = default;
            Vector sp = default;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[i + 0];
                var p1 = points[i + 1];
                var delta = p0.Point - p1.Point;
                //var span = Math.Min(p0.Timestamp - p1.Timestamp, 100);
                var span = p0.Timestamp - p1.Timestamp;
                if (0 < span)
                {
                    var speed = delta / span;
                    totalSpan += span;
                    speedSum += speed * span;
                    Trace($"{i}: Speed = {speed:f2}, Span = {span}");
                    sp = speed;
                }
                if (totalSpan > 100)
                {
                    break;
                }
            }

            if (totalSpan <= 0) return default;
            var lastSpeed = speedSum / totalSpan;

            Trace($"LastSpeed = {lastSpeed.Length:f2} ({lastSpeed:f2}), TotalSpan = {totalSpan}");
            return lastSpeed;
        }

        public PointInertia GetInertia()
        {
            var limitSpeed = 40.0;
            var minimumSpeed = 1.0;
            var a = 0.02;

            var speed = GetSpeed();

            // speed limit
            if (speed.LengthSquared > limitSpeed * limitSpeed)
            {
                Trace($"Speed Limited: {speed.Length} => {limitSpeed}");
                speed = speed * (limitSpeed / speed.Length);
            }

            var v = speed.Length;

            if (v <= minimumSpeed) return default;

            var t = v / a;
            var s = v * t - a * t * t * 0.5;

            //t = Math.Max(t, 200);
            var span = TimeSpan.FromMilliseconds(t);
            var delta = speed * s / speed.Length;
            Trace($"Inertia: Delta = {delta:f2}, Span = {span.TotalMilliseconds}");

            return new PointInertia(delta, span);
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{nameof(Speedometer)}: {string.Format(s, args)}");
        }
    }

    /// <summary>
    /// 座標慣性
    /// </summary>
    /// <param name="Delta">慣性移動量</param>
    /// <param name="Span">慣性移動時間</param>
    public record struct PointInertia(Vector Delta, TimeSpan Span);

    /// <summary>
    /// 座標記録
    /// </summary>
    /// <param name="Point">座標</param>
    /// <param name="Timestamp">タイムスタンプ</param>
    public record struct PointRecord(Point Point, int Timestamp);
}

