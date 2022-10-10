using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public enum ResizeInterpolation
    {
        NearestNeighbor,
        Average,
        Linear,
        Quadratic,
        Hermite,
        Mitchell,
        CatmullRom,
        Cubic,
        CubicSmoother,
        Lanczos, // default.
        Spline36,
    }

    public static class ResizeInterpolationExtensions
    {
        public static List<ResizeInterpolation> ResizeInterpolationList { get; } =
            Enum.GetValues(typeof(ResizeInterpolation)).Cast<ResizeInterpolation>().Where(e => e != ResizeInterpolation.NearestNeighbor).ToList();
    }
}
