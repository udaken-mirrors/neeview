using System.Windows;
using System.Windows.Media.TextFormatting;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    public class TransformControlFactory
    {
        private BookContext _context;
        private ViewTransformContext _viewContext;
        private LoupeTransformContext _loupeContext;
        private ScrollLock _scrollLock;


        public TransformControlFactory(BookContext context, ViewTransformContext viewContext, LoupeTransformContext loupeContext, ScrollLock scrollLock)
        {
            _context = context;
            _viewContext = viewContext;
            _loupeContext = loupeContext;
            _scrollLock = scrollLock;
        }

        public ITransformControl Create(PageFrameContainer container)
        {
            if (_context.IsStaticFrame)
            {
                var containerRect = container.Rect.Size.ToRect();
                return new ContentTransformControl(container, containerRect, _scrollLock);
            }
            else
            {
                return new ViewTransformControl(container, _viewContext, _scrollLock);
            }
        }

        public ITransformControl CreateLoupe(PageFrameContainer container)
        {
            return new LoupeTransformControl(container, _loupeContext);
        }

        public Rect CreateContentRect(PageFrameContainer container)
        {
            if (_context.IsStaticFrame)
            {
                return container.GetContentRect();
            }
            else
            {
                return container.Rect;
            }
        }

        public Rect CreateViewRect(Rect viewRect)
        {
            if (_context.IsStaticFrame)
            {
                // TODO: Contaienr から求めるべきでは？
                return viewRect.Size.ToRect();
            }
            else
            {
                return viewRect;
            }
        }
    }





}