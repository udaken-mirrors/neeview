using System;
using System.ComponentModel;
using System.Windows;
using PageRange = NeeView.PageRange;


namespace NeeView.PageFrames
{
    public interface IPageFrameContent : IDisposable, INotifyTransformChanged  
    {
        public event EventHandler? ContentSizeChanged;

        UIElement? Content { get; }
        IPageFrameTransform Transform { get; }

        PageFrameActivity Activity { get; }
        bool IsLocked { get; }
        PageRange FrameRange { get; }
        public bool IsFirstFrame { get; }
        public bool IsLastFrame { get; }
        PageFrameDartyLevel DartyLevel { get; set; }

        Rect GetContentRect();
        Size GetFrameSize();
    }


    public class DummyPageFrameContent : IPageFrameContent
    {
        public DummyPageFrameContent(PageFrameActivity activity)
        {
            Activity = activity;
        }

        public virtual PageRange FrameRange => PageRange.Empty;

        public IPageFrameTransform Transform => new DummyPageFrameTransform();

        public virtual UIElement? Content => null;

        public PageFrameActivity Activity { get; }

        public virtual bool IsLocked => false;
       
        public bool IsFirstFrame => false;

        public bool IsLastFrame => false;

        public PageFrameDartyLevel DartyLevel
        {
            get { return PageFrameDartyLevel.Clean; }
            set { }
        }

        public event TransformChangedEventHandler? TransformChanged;
        public event EventHandler? ContentSizeChanged;

        public void Dispose()
        {
        }

        public void SetDarty(PageFrameDartyLevel level)
        {
        }

        public Rect GetContentRect()
        {
            return new Rect();
        }

        public Size GetFrameSize()
        {
            return new Size();
        }

        public override string ToString()
        {
            return "Dummy";
        }
    }

}
