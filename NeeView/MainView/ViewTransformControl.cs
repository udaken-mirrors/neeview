using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ViewTransformControl : IViewTransformControl
    {
        private readonly MainViewComponent _viewComponent;
        private readonly ScrollPageController _scrollPageControl;

        public ViewTransformControl(MainViewComponent viewComponent, ScrollPageController scrollPageControl)
        {
            _viewComponent = viewComponent;
            _scrollPageControl = scrollPageControl;
        }

        public void FlipHorizontal(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipHorizontal(isFlip);
        }

        public void FlipVertical(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipVertical(isFlip);
        }

        public void ToggleFlipHorizontal()
        {
            _viewComponent.DragTransformControl.ToggleFlipHorizontal();
        }

        public void ToggleFlipVertical()
        {
            _viewComponent.DragTransformControl.ToggleFlipVertical();
        }

        public void ResetContentSizeAndTransform()
        {
            _viewComponent.ContentCanvas.ResetContentSizeAndTransform(new ResetTransformCondition(true));
        }

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateLeft(parameter);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateRight(parameter);
        }

        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleDown(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleUp(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollUp(parameter);
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollDown(parameter);
        }

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollLeft(parameter);
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollRight(parameter);
        }


        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeUp(parameter);
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeDown(parameter);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.PrevScrollPage(sender, parameter);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.NextScrollPage(sender, parameter);
        }

    }
}
