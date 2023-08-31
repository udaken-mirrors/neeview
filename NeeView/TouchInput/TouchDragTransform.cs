using NeeLaboratory;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// タッチ操作用トランスフォーム
    /// </summary>
    public class TouchDragTransform
    {
        public TouchDragTransform()
        {
        }

        public TouchDragTransform(Vector trans, double angle, double scale) : this(trans, angle, scale, default)
        {
        }

        public TouchDragTransform(Vector trans, double angle, double scale, Vector center)
        {
            Trans = trans;
            Angle = angle;
            Scale = scale;
            Center = center;
        }


        public Vector Trans { get; init; }
        public double Angle { get; init; }
        public double Scale { get; init; }

        // 回転、拡大縮小の中心
        public Vector Center { get; init; }


        public TouchDragTransform Clone()
        {
            return (TouchDragTransform)this.MemberwiseClone();
        }

#if false
        public TouchDragTransform Add(TouchDragTransform m)
        {
            return new TouchDragTransform(this.Trans + m.Trans, this.Angle + m.Angle, this.Scale + m.Scale, (this.Center + m.Center) * 0.5);
        }

        public TouchDragTransform Sub(TouchDragTransform m)
        {
            return new TouchDragTransform(this.Trans - m.Trans, this.Angle - m.Angle, this.Scale - m.Scale, (this.Center + m.Center) * 0.5);
        }

        internal void Multiply(double v)
        {
            return new TouchDragTransform(this.Trans - m.Trans, this.Angle - m.Angle, this.Scale - m.Scale, (this.Center + m.Center) * 0.5);
        }

        public static TouchDragTransform Sub(TouchDragTransform m0, TouchDragTransform m1)
        {
            var m = m0.Clone();
            m.Sub(m1);
            return m;
        }

        public static TouchDragTransform Lerp(TouchDragTransform m0, TouchDragTransform m1, double t)
        {
            t = MathUtility.Clamp(t, 0.0, 1.0);

            return new TouchDragTransform()
            {
                Trans = m0.Trans + (m1.Trans - m0.Trans) * t,
                Angle = m0.Angle + (m1.Angle - m0.Angle) * t,
                Scale = m0.Scale + (m1.Scale - m0.Scale) * t,
            };
        }
#endif
    }
}
