using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame のサイズに影響するパラメータと計算
    /// </summary>
    // NOTE: NeeView exist
    // TODO: ２つ並べたコンテンツのサイズをあわせる計算。 PageSource に Scale を保持させる
    public class ContentSizeCalculator
    {
        private readonly IContentSizeCalculatorProfile _profile;

        public ContentSizeCalculator(IContentSizeCalculatorProfile profile)
        {
            _profile = profile;
        }

        public Size CanvasSize => _profile.CanvasSize;
        public AutoRotateType AutoRotateType => _profile.AutoRotateType;
        public bool AllowFileContentAutoRotate => _profile.AllowFileContentAutoRotate;
        public PageStretchMode StretchMode => _profile.StretchMode;
        public double ContentsSpace => _profile.ContentsSpace;
        public bool AllowEnlarge => _profile.AllowEnlarge;
        public bool AllowReduce => _profile.AllowReduce;

        /// <summary>
        /// 自動回転を求める
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public double CalcAutoRotate(Size size, bool isFileContent)
        {
            if (isFileContent && !AllowFileContentAutoRotate)
            {
                return 0.0;
            }

            if (AutoRotateType == AutoRotateType.None)
            {
                return 0.0;
            }

            var isContentLandscape = AspectRatioTools.IsLandscape(size);
            var isCanvasLandscape = CanvasSize.Width >= CanvasSize.Height;
            return isContentLandscape != isCanvasLandscape ? AutoRotateType.ToAngle() : 0.0;
        }

        /// <summary>
        /// ストレッチスケールを求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public double CalcStretchScale(Size size, RotateTransform rotate)
        {
            //return CalcStretchScale(StretchMode, GetFillScale(size, rotate), size);
            var scales = GetFillScale(size, rotate);

            var max = Math.Max(scales.X, scales.Y);
            var min = Math.Min(scales.X, scales.Y);
            return min;
        }

        /// <summary>
        /// ストレッチスケールを求める。PageFrame用
        /// </summary>
        /// <param name="size">コンテンツサイズ</param>
        /// <param name="span">コンテンツ間スペース</param>
        /// <param name="rotate">自動回転</param>
        /// <returns></returns>
        public double CalcFrameStretchScale(Size size, double span, RotateTransform rotate)
        {
            return CalcStretchScale(StretchMode, GetFillScaleWithSpan(size, span, rotate.Angle), size);
        }


        private double CalcStretchScale(PageStretchMode stretchMode, Vector scales, Size size)
        {
            return stretchMode switch
            {
                PageStretchMode.None => OneScale(),
                PageStretchMode.Uniform => Math.Min(scales.X, scales.Y),
                PageStretchMode.UniformToFill => Math.Max(scales.X, scales.Y),
                PageStretchMode.UniformToHorizontal => scales.X,
                PageStretchMode.UniformToVertical => scales.Y,
                PageStretchMode.UniformToSize => GetFillSizeScale(size),
                _ => throw new NotImplementedException(),
            };
        }

        private double GetFillSizeScale(Size size)
        {
            if (size.Width <= 0.0 || size.Height <= 0.0) return 1.0;
            var s0 = CanvasSize.Width * CanvasSize.Height;
            var s1 = size.Width * size.Height;
            var scale = AllowScale(Math.Sqrt(s0 / s1));
            return scale;
        }


        /// <summary>
        /// Calculate fill-scale
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        private Vector GetFillScale(Size size, RotateTransform rotate)
        {
            if (size.Width <= 0.0 || size.Height <= 0.0) return new Vector(1.0, 1.0);
            var bounds = rotate.TransformBounds(size.ToRect());
            var scaleX = AllowScale(CanvasSize.Width / bounds.Width);
            var scaleY = AllowScale(CanvasSize.Height / bounds.Height);
            return new Vector(scaleX, scaleY);
        }

        /// <summary>
        /// Calculate fill-scale for PageFrame
        /// </summary>
        /// <param name="size"></param>
        /// <param name="span">PageContent 間の距離。スケールに影響されない固定値</param>
        /// <param name="angle">自動回転角度。90度単位のみ対応</param>
        /// <returns>回転前に適用されるScale</returns>
        private Vector GetFillScaleWithSpan(Size size, double span, double angle)
        {
            Debug.Assert(Math.Abs(angle % 90) < 1.0, "Only 90-degree units are supported.");

            if (size.Width <= 0.0 || size.Height <= 0.0) return new Vector(1.0, 1.0);
            var radian = Math.PI * angle / 180.0;
            var isTranspose = Math.Abs(Math.Sin(radian)) > 0.5;
            if (isTranspose)
            {
                var scaleY = AllowScale((CanvasSize.Height - span) / size.Width);
                var scaleX = AllowScale(CanvasSize.Width / size.Height);
                return new Vector(scaleX, scaleY);
            }
            else
            {
                var scaleX = AllowScale((CanvasSize.Width - span) / size.Width);
                var scaleY = AllowScale(CanvasSize.Height / size.Height);
                return new Vector(scaleX, scaleY);
            }
        }

        private double AllowScale(double scale)
        {
            if (!AllowEnlarge)
            {
                scale = Math.Min(scale, OneScale());
            }
            if (!AllowReduce)
            {
                scale = Math.Max(scale, OneScale());
            }
            return scale;
        }

        public double[] CalcContentScale(IEnumerable<Size> contents)
        {
            if (!contents.Any())
            {
                return Array.Empty<double>();
            }

            if (contents.Count() == 1)
            {
                return new double[] { 1.0 };
            }

            if (StretchMode == PageStretchMode.None)
            {
                return contents.Select(e => 1.0).ToArray();
            }

            var height = contents.Max(e => e.Height);
            return contents.Select(e => height / e.Height).ToArray();
        }

        // DPIを加味した基準スケール
        private double OneScale() => 1.0 / _profile.DpiScale.ToFixedScale().DpiScaleX;
    }

    public static class SizeExtensions
    {
        public static Rect ToRect(this Size size)
        {
            return new Rect(-size.Width * 0.5, -size.Height * 0.5, size.Width, size.Height);
        }
    }
}
