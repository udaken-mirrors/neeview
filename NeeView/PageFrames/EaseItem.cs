using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// EasingFunction の適用範囲情報
    /// </summary>
    /// <param name="Ease">Easing関数</param>
    /// <param name="S">移動量</param>
    /// <param name="T">移動時間</param>
    /// <param name="S0">開始位置</param>
    /// <param name="T0">開始時間</param>
    public record class EaseItem(IEasingFunction Ease, double S, double T, double S0, double T0)
    {
        public double S1 => S0 + S;
        public double T1 => T0 + T;
    }




}