#if false
namespace NeeView
{
    public class ViewPropertyControl : IViewPropertyControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewPropertyControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return _viewComponent.ContentCanvas.TestStretchMode(mode, isToggle);
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            _viewComponent.ContentCanvas.SetStretchMode(mode, isToggle);
        }

        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchMode(parameter);
        }

        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchModeReverse(parameter);
        }

        public bool GetAutoRotateLeft()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public void SetAutoRotateLeft(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = flag;
        }

        public void ToggleAutoRotateLeft()
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = !_viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public bool GetAutoRotateRight()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateRight;
        }

        public void SetAutoRotateRight(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = flag;
        }

        public void ToggleAutoRotateRight()
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = !_viewComponent.ContentCanvas.IsAutoRotateRight;
        }
    }
}
#endif

using System;

namespace NeeView
{
    public class ViewPropertyControl : IViewPropertyControl
    {
        private ViewConfig _viewConfig;

        public ViewPropertyControl(ViewConfig viewConfig)
        {
            _viewConfig = viewConfig;
        }

        public bool GetAutoRotateLeft()
        {
            return _viewConfig.AutoRotate == AutoRotateType.Left;
        }

        public bool GetAutoRotateRight()
        {
            return _viewConfig.AutoRotate == AutoRotateType.Right;
        }

        public void SetAutoRotateLeft(bool flag)
        {
            _viewConfig.AutoRotate = flag ? AutoRotateType.Left : AutoRotateType.None;
        }

        public void SetAutoRotateRight(bool flag)
        {
            _viewConfig.AutoRotate = flag ? AutoRotateType.Right : AutoRotateType.None;
        }

        public void ToggleAutoRotateLeft()
        {
            _viewConfig.AutoRotate = _viewConfig.AutoRotate != AutoRotateType.Left ? AutoRotateType.Left : AutoRotateType.None;
        }

        public void ToggleAutoRotateRight()
        {
            _viewConfig.AutoRotate = _viewConfig.AutoRotate != AutoRotateType.Right ? AutoRotateType.Right : AutoRotateType.None;
        }


        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter)
        {
            PageStretchMode mode = _viewConfig.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var next = (int)mode + 1;
                if (!parameter.IsLoop && next >= length) return _viewConfig.StretchMode;
                mode = (PageStretchMode)(next % length);
                if (parameter.GetStretchModeDictionary()[mode]) return mode;
            }
            while (count++ < length);
            return _viewConfig.StretchMode;
        }

        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter)
        {
            PageStretchMode mode = _viewConfig.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var prev = (int)mode - 1;
                if (!parameter.IsLoop && prev < 0) return _viewConfig.StretchMode;
                mode = (PageStretchMode)((prev + length) % length);
                if (parameter.GetStretchModeDictionary()[mode]) return mode;
            }
            while (count++ < length);
            return _viewConfig.StretchMode;
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            _viewConfig.StretchMode = GetFixedStretchMode(mode, isToggle);
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return mode == GetFixedStretchMode(mode, isToggle);
        }

        private PageStretchMode GetFixedStretchMode(PageStretchMode mode, bool isToggle)
        {
            if (isToggle && _viewConfig.StretchMode == mode)
            {
                return (mode == PageStretchMode.None) ? _viewConfig.ValidStretchMode : PageStretchMode.None;
            }
            else
            {
                return mode;
            }
        }

    }
}