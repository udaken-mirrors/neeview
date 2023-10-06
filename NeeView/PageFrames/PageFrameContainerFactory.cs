﻿using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{

    public class PageFrameContainerFactory
    {
        // TODO: container recycle ... ここではないな

        private PageFrameContext _context;
        private PageFrameTransformMap _transformMap;
        private LoupeTransformContext _loupeContext;
        private ViewContentFactory _viewContentFactory;
        private BaseScaleTransform _baseScaleTransform;

        public PageFrameContainerFactory(PageFrameContext context, PageFrameTransformMap transformMap, ViewSourceMap viewSourceMap, LoupeTransformContext loupeContext, BaseScaleTransform baseScaleTransform)
        {
            _context = context;
            _transformMap = transformMap;
            _loupeContext = loupeContext;
            _viewContentFactory = new ViewContentFactory(viewSourceMap);
            _baseScaleTransform = baseScaleTransform;
        }


        public PageFrameContainer Create(PageFrame frame)
        {
            var activity = new PageFrameActivity();
            var key = PageFrameTransformTool.CreateKey(frame);

            var rawTransform = _transformMap.ElementAt(key);
            if (!_context.ViewConfig.IsKeepPageTransform)
            {
                rawTransform.Clear();
            }

            var transform = new PageFrameTransformAccessor(_transformMap, rawTransform);
            var content = new PageFrameContent(_viewContentFactory, _context, frame, activity, transform, _loupeContext, _baseScaleTransform);
            var container = new PageFrameContainer(content, activity, _context.ViewScrollContext);
            return container;
        }

        public void Update(PageFrameContainer container, PageFrame frame)
        {
            if (container.Content is PageFrameContent frameContent && frameContent.PageFrame.IsMatch(frame) && container.DirtyLevel < PageFrameDirtyLevel.Replace )
            {
                frameContent.SetSource(frame);
                container.UpdateFrame();
            }
            else
            {
                var activity = container.Activity;
                var key = PageFrameTransformTool.CreateKey(frame);
                var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(key));
                var content = new PageFrameContent(_viewContentFactory, _context, frame, activity, transform, _loupeContext, _baseScaleTransform);
                container.Content = content;
                container.UpdateFrame();
            }
        }

    }
}
