using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class TouchContext
    {
        public TouchContext(StylusDevice stylusDevice, Point startPoint, int startTimestamp)
        {
            StylusDevice = stylusDevice;
            StartPoint = startPoint;
            StartTimestamp = startTimestamp;
        }


        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// ドラッグ開始時間
        /// </summary>
        public int StartTimestamp { get; set; }

        /// <summary>
        /// デバイス
        /// </summary>
        public StylusDevice StylusDevice { get; set; }
    }
}
