﻿using System.Windows;

namespace NeeView.PageFrames
{
    public interface IContentSizeCalculatorProfile
    {
        public double ContentsSpace { get; }
        public PageStretchMode StretchMode { get; }
        public AutoRotateType AutoRotateType { get; }
        public bool AllowFileContentAutoRotate { get; }
        public bool AllowEnlarge { get; }
        public bool AllowReduce { get; }
        public Size CanvasSize { get; }
        public DpiScale DpiScale { get; }
    }
}
