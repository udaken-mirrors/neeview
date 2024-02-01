using System;
using System.Diagnostics;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 運動学の演算
    /// </summary>
    public static class Kinematics
    {
        /// <summary>
        /// 等加速度運動(UAM)：初速度を求める
        /// </summary>
        public static double GetFirstVelocity(double a, double s, double t)
        {
            Debug.Assert(t > 0.0);
            return (s / t) - (a * t * 0.5);
        }

        /// <summary>
        /// 等加速度運動：到達速度を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GetVelocity(double v, double a, double t)
        {
            return v + a * t;
        }

        /// <summary>
        /// 等加速度運動：加速度を求める
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double GetAccelerate(double v0, double v1, double s)
        {
            return (v1 * v1 - v0 * v0) / (2.0 * s);
        }

        /// <summary>
        /// 等加速度運動：距離を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GetSpan(double v, double a, double t)
        {
            return v * t + 0.5 * a * t * t;
        }

        /// <summary>
        /// 等加速度運動：到達時間を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double GetTime(double v, double a, double s)
        {
            Debug.Assert(a != 0.0);

            var x = v * v + 2.0 * a * s;
            if (Math.Abs(x) < 0.001) x = 0.0;

            var y = Math.Sqrt(x);
            var t1 = (-v + y) / a;
            var t2 = (-v - y) / a;
            if (t1 < 0) return t2;
            if (t2 < 0) return t1;
            return Math.Min(t1, t2);
        }

        /// <summary>
        /// 等加速度運動：停止するまでの時間を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double GetStopTime(double v, double a)
        {
            Debug.Assert(a < 0.0, "Must be a negative value.");
            return v / (-a);
        }
    }




}
