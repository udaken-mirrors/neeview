using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace NeeView.PageFrames
{
    public class TerminalPageFrameContent : DummyPageFrameContent
    {
        //private Rectangle _rectangle;

        public TerminalPageFrameContent(PageRange frameRange, PageFrameActivity activity) : base(activity)
        {
            FrameRange = frameRange;

#if false
            // [DEV]
            _rectangle = new Rectangle()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = Brushes.Red,
                Margin = new Thickness(-2.0),
            };
#endif
        }

        public override FrameworkElement? Content => null;

        public override bool IsLocked => true;
        public override PageRange FrameRange { get; }

        public override string ToString()
        {
            return "T" + base.ToString();
        }
    }
}
