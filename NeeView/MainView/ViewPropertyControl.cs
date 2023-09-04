using NeeLaboratory.Generators;
using System;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ViewPropertyControl : IViewPropertyControl
    {
        private ViewConfig _viewConfig;

        public ViewPropertyControl(ViewConfig viewConfig)
        {
            _viewConfig = viewConfig;

            _viewConfig.SubscribePropertyChanged(nameof(ViewConfig.AutoRotate), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsAutoRotateLeft));
                RaisePropertyChanged(nameof(IsAutoRotateRight));
            });
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsAutoRotateLeft
        {
            get => GetAutoRotateLeft();
            set => SetAutoRotateLeft(value);
        }

        public bool IsAutoRotateRight
        {
            get => GetAutoRotateRight();
            set => SetAutoRotateRight(value);
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