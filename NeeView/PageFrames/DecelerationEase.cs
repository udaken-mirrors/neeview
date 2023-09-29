using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 減速曲線
    /// </summary>
    /// <remarks>
    /// 等加速度運動方程式を使用
    /// </remarks>
    public class DecelerationEase : EasingFunctionBase
    {
        public const double DefaultAcceleration = -0.01;


        public DecelerationEase()
        {
            EasingMode = EasingMode.EaseIn;
        }

        /// <summary>
        /// 加速度。減速なので負の値
        /// </summary>
        public double Acceleration
        {
            get { return (double)GetValue(AccelerationProperty); }
            set { SetValue(AccelerationProperty, value); }
        }

        public static readonly DependencyProperty AccelerationProperty =
            DependencyProperty.Register("Acceleration", typeof(double), typeof(DecelerationEase), new PropertyMetadata(DefaultAcceleration));

        /// <summary>
        /// 範囲下限。0.0 が減速開始初期位置。
        /// </summary>
        public double MinRate
        {
            get { return (double)GetValue(MinRateProperty); }
            set { SetValue(MinRateProperty, value); }
        }

        public static readonly DependencyProperty MinRateProperty =
            DependencyProperty.Register("MinRate", typeof(double), typeof(DecelerationEase), new PropertyMetadata(0.0));

        /// <summary>
        /// 範囲上限。1.0 で減速による停止状態の位置。
        /// </summary>
        public double MaxRate
        {
            get { return (double)GetValue(MaxRateProperty); }
            set { SetValue(MaxRateProperty, value); }
        }

        public static readonly DependencyProperty MaxRateProperty =
            DependencyProperty.Register("MaxRate", typeof(double), typeof(DecelerationEase), new PropertyMetadata(1.0));


        protected override Freezable CreateInstanceCore()
        {
            return new DecelerationEase();
        }

        protected override double EaseInCore(double normalizedTime)
        {
            Debug.Assert(MinRate < MaxRate);

            var v = 1.0;
            var a = Acceleration;
            var t = Math.Abs(v / a);

            var rate = MinRate + (MaxRate - MinRate) * normalizedTime;
            var s = Kinematics.GetSpan(v, a, rate * t);
            var s0 = Kinematics.GetSpan(v, a, MinRate * t);
            var s1 = Kinematics.GetSpan(v, a, MaxRate * t);

            var result = (s - s0) / (s1 - s0);
            if (double.IsNaN(result)) result = 1.0;

            //Debug.WriteLine($"MyEase: {normalizedTime:f2} -> {result:f2}: v={v:f2}, a={a:f2}, t={t:f2}, rate={rate:f2}");
            return result;
        }
    }




}