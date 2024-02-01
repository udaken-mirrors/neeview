using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 座標移動用 EasingFunction のセット
    /// </summary>
    public class EaseSet : IEaseSet
    {
        public EaseSet(Vector delta, double ms, IEasingFunction easeX, IEasingFunction easeY, Vector v0, Vector v1)
        {
            Delta = delta;
            Milliseconds = ms;
            EaseX = easeX;
            EaseY = easeY;
            V0 = v0;
            V1 = v1;
        }

        public Vector Delta { get; }
        public double Milliseconds { get; }
        public IEasingFunction EaseX { get; }
        public IEasingFunction EaseY { get; }

        /// <summary>
        /// 初期速度
        /// </summary>
        public Vector V0 { get; }

        /// <summary>
        /// 終了速度
        /// </summary>
        public Vector V1 { get; }
    }




}
