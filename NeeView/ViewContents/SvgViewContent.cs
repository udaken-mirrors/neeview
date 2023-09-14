using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Threading;

namespace NeeView
{
    public class SvgViewContent : ImageViewContent
    {
        public SvgViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
        }
    }
}
