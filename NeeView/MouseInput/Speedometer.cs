#define LOCAL_DEBUG

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
            _points = new RingList<PointRecord>(5);
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
                    var cs = Math.Min(span, 64 - totalSpan);
                    if (cs <= 0) break;

                    var speed = delta / cs;
                    totalSpan += cs;
                    speedSum += speed * cs;
                    Trace($"{i}: Speed = {speed:f2}, Span = {cs}");
                    sp = speed;
                }
            }

            if (totalSpan <= 0) return default;
            var lastSpeed = speedSum / totalSpan;

            Trace($"LastSpeed = {lastSpeed.Length:f2} ({lastSpeed:f2}), TotalSpan = {totalSpan}");
            return lastSpeed;
        }

        [Obsolete]
        public PointInertia GetInertia()
        {
            var inertiaMath = new InertiaMath();
            return inertiaMath.CalcInertia(GetSpeed());
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
    [Obsolete]
    public record struct PointInertia(Vector Velocity, Vector Delta, TimeSpan Span);

    /// <summary>
    /// 座標記録
    /// </summary>
    /// <param name="Point">座標</param>
    /// <param name="Timestamp">タイムスタンプ</param>
    public record struct PointRecord(Point Point, int Timestamp);


    [Obsolete]
    public class InertiaMath
    {
        public InertiaMath()
        {
        }

        public InertiaMath(double deceleration, double minSpeed, double maxSpeed)
        {
            Deceleration = deceleration;
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
        }

        /// <summary>
        /// 減速係数
        /// </summary>
        public double Deceleration { get; set; } = 0.01;

        /// <summary>
        /// 最小速度。これ以下では慣性は発生しない
        /// </summary>
        public double MinSpeed { get; set; } = 1.0;

        /// <summary>
        /// 最大速度。この速度を上限とした慣性になる
        /// </summary>
        public double MaxSpeed { get; set; } = 40.0;

        /// <summary>
        /// 慣性情報を取得
        /// </summary>
        /// <param name="speed">初速度(dot/ms)</param>
        /// <returns></returns>
        public PointInertia CalcInertia(Vector speed)
        {
            // speed limit
            if (speed.LengthSquared > MaxSpeed * MaxSpeed)
            {
                Trace($"Speed Limited: {speed.Length} => {MaxSpeed}");
                speed = speed * (MaxSpeed / speed.Length);
            }

            var v = speed.Length;

            if (v <= MinSpeed) return default;

            var t = v / Deceleration;
            var s = v * t - Deceleration * t * t * 0.5;
            //t = Math.Max(t, 200);
            var span = TimeSpan.FromMilliseconds(t);
            var delta = speed * s / speed.Length;
            Trace($"Inertia: Delta = {delta:f2}, Span = {span.TotalMilliseconds}");

            return new PointInertia(speed, delta, span);
        }

        [Conditional("LOCAL_DEBUG")]
        private static void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{nameof(Speedometer)}: {string.Format(s, args)}");
        }
    }

}

