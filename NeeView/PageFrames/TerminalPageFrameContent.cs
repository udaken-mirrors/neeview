using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using PageRange = NeeView.PageRange;


namespace NeeView.PageFrames
{
    public class TerminalPageFrameContent : DummyPageFrameContent
    {
        private Rectangle _rectangle;

        public TerminalPageFrameContent(PageRange frameRange, PageFrameActivity activity) : base(activity)
        {
            FrameRange = frameRange;

            // [DEV]
            _rectangle = new Rectangle()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = Brushes.Red,
                Margin = new Thickness(-2.0),
            };
        }

        public override UIElement? Content => _rectangle;

        public override bool IsLocked => true;
        public override PageRange FrameRange { get; }

        public override string ToString()
        {
            return "T" + base.ToString();
        }
    }
}
