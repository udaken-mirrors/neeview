//#define LOCAL_DEBUG

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 移動速度計測
    /// </summary>
    public class Speedometer
    {
        private readonly RingList<PointRecord> _points;

        public Speedometer()
        {
            _points = new RingList<PointRecord>(5);
        }

        /// <summary>
        /// 計測座標初期化
        /// </summary>
        public void Reset()
        {
            _points.Clear();
        }

        /// <summary>
        /// 現在の計測座標追加
        /// </summary>
        /// <param name="point"></param>
        public void Add(Point point)
        {
            var timestamp = System.Environment.TickCount;

            if (!_points.Any())
            {
                _points.Add(new PointRecord(point, timestamp));
            }
            else
            {
                var last = _points.Last();
                if (timestamp == last.Timestamp)
                {
                    last.Point = point;
                }
                else
                {
                    _points.Add(new PointRecord(point, timestamp));
                }
            }
        }

        /// <summary>
        /// 計測座標確定
        /// </summary>
        public void Touch()
        {
            if (!_points.Any()) return;
            Add(_points.Last().Point);
        }

        /// <summary>
        /// 計測座標から速度を求める
        /// </summary>
        /// <remarks>
        /// スパイクデータを軽減するため一定期間の平均速度を求める
        /// </remarks>
        /// <returns></returns>
        public Vector GetVelocity()
        {
            if (!_points.Any()) return default;

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
            var velocity = speedSum / totalSpan;

            Trace($"Velocity = {velocity.Length:f2} ({velocity:f2}), TotalSpan = {totalSpan}");
            return velocity;
        }


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{nameof(Speedometer)}: {string.Format(s, args)}");
        }
    }

    /// <summary>
    /// 座標記録
    /// </summary>
    /// <param name="Point">座標</param>
    /// <param name="Timestamp">タイムスタンプ</param>
    public record struct PointRecord(Point Point, int Timestamp);
}

