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
        private BaseScaleTransform _baseScaleTransform;

        public PageFrameContainerFactory(IStaticFrame staticFrameProfile, PageFrameTransformMap transformMap, ViewSourceMap viewSourceMap, LoupeTransformContext loupeContext, BaseScaleTransform baseScaleTransform)
        {
            _staticFrameProfile = staticFrameProfile;
            _transformMap = transformMap;
            _loupeContext = loupeContext;
            _viewContentFactory = new ViewContentFactory(viewSourceMap);
            _baseScaleTransform = baseScaleTransform;
        }


        public PageFrameContainer Create(PageFrame frame)
        {
            var activity = new PageFrameActivity();
            var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(frame.FrameRange));
            var content = new PageFrameContent(_viewContentFactory, _staticFrameProfile, frame, activity, transform, _loupeContext, _baseScaleTransform);
            var container = new PageFrameContainer(content, activity);
            return container;
        }

        public void Update(PageFrameContainer container, PageFrame frame)
        {
            if (container.Content is PageFrameContent frameContent && frameContent.PageFrame.IsMatch(frame) && container.DirtyLevel < PageFrameDartyLevel.Replace )
            {
                frameContent.SetSource(frame);
                container.UpdateFrame();
            }
            else
            {
                var activity = container.Activity;
                var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(frame.FrameRange));
                var content = new PageFrameContent(_viewContentFactory, _staticFrameProfile, frame, activity, transform, _loupeContext, _baseScaleTransform);
                container.Content = content;
            }
        }

    }
}
