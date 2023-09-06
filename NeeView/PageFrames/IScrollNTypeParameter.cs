using System;

namespace NeeView.PageFrames
{
#if false
    public enum NScrollType
    {
        NType,
        ZType,
        Diagonal,
    }

    public enum LineBreakStopMode
    {
        Line,
        Page,
    }

    public interface IScrollNTypeParameter
    {
        /// <summary>
        /// スクロールの種類
        /// </summary>
        NScrollType ScrollType { get; set; }

        /// <summary>
        /// スクロール移動量の割合
        /// </summary>
        double Scroll { get; set; }

        /// <summary>
        /// スクロール時間
        /// </summary>
        double ScrollDuration { get; set; }

        /// <summary>
        /// 改行遅延時間
        /// </summary>
        double LineBreakStopTime { get; set; }
    }
#endif

    public interface IScrollNTypeEndMargin
    {
        /// <summary>
        /// 終端判定マージン
        /// </summary>
        double EndMargin { get; set; }
    }

    // [開発用]
    public class ScrollNTypeParameter : IScrollNTypeParameter, IScrollNTypeEndMargin
    {
        public NScrollType ScrollType { get; set; } = NScrollType.NType;
        public double Scroll { get; set; } = 1.0;
        [Obsolete]
        public double ScrollDuration { get; set; } = 0.2;
        public double LineBreakStopTime { get; set; }
        public double EndMargin { get; set; } = 10.0;
    }


}
