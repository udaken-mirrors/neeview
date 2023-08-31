using System;
using System.ComponentModel;
using System.Windows;
using PageRange = NeeView.PageRange;


namespace NeeView.PageFrames
{
    public interface IPageFrameContent : IDisposable, INotifyTransformChanged  
    {
        public event EventHandler? ViewContentChanged;
        public event EventHandler? ContentSizeChanged;
        FrameworkElement? Content { get; }
        IPageFrameTransform Transform { get; }

        PageFrameActivity Activity { get; }
        bool IsLocked { get; }
        PageRange FrameRange { get; }
        bool IsFirstFrame { get; }
        bool IsLastFrame { get; }
        PageFrameDartyLevel DirtyLevel { get; set; }
        bool IsStaticFrame { get; }

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

        public virtual FrameworkElement? Content => null;

        public PageFrameActivity Activity { get; }

        public virtual bool IsLocked => false;
       
        public bool IsFirstFrame => false;

        public bool IsLastFrame => false;

        public PageFrameDartyLevel DirtyLevel
        {
            get { return PageFrameDartyLevel.Clean; }
            set { }
        }

        public bool IsStaticFrame => false;

        public event TransformChangedEventHandler? TransformChanged;
        public event EventHandler? ViewContentChanged;
        public event EventHandler? ContentSizeChanged;

        public void Dispose()
        {
        }

        public void SetDirty(PageFrameDartyLevel level)
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
