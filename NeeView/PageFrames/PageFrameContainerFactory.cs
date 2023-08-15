using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{

    public class PageFrameContainerFactory
    {
        // TODO: container recycle ... ここではないな

        private IStaticFrame _staticFrameProfile;
        private PageFrameTransformMap _transformMap;
        private LoupeTransformContext _loupeContext;
        private ViewContentFactory _viewContentFactory;


        public PageFrameContainerFactory(IStaticFrame staticFrameProfile, PageFrameTransformMap transformMap, ViewSourceMap viewSourceMap, LoupeTransformContext loupeContext)
        {
            _staticFrameProfile = staticFrameProfile;
            _transformMap = transformMap;
            _loupeContext = loupeContext;
            _viewContentFactory = new ViewContentFactory(viewSourceMap);
        }


        public PageFrameContainer Create(PageFrame frame)
        {
            var activity = new PageFrameActivity();
            var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(frame.FrameRange));
            var content = new PageFrameContent(_viewContentFactory, _staticFrameProfile, frame, activity, transform, _loupeContext);
            var container = new PageFrameContainer(content, activity);
            return container;
        }

        public void Update(PageFrameContainer container, PageFrame frame)
        {
            if (container.Content is PageFrameContent frameContent && frameContent.PageFrame.IsMatch(frame) && container.DartyLevel < PageFrameDartyLevel.Replace )
            {
                frameContent.SetSource(frame);
                container.UpdateFrame();
            }
            else
            {
                var activity = container.Activity;
                var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(frame.FrameRange));
                var content = new PageFrameContent(_viewContentFactory, _staticFrameProfile, frame, activity, transform, _loupeContext);
                container.Content = content;
            }
        }

    }
}
