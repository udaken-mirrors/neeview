using NeeLaboratory.Generators;
using System;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ViewPropertyControl : IViewPropertyControl
    {
        private readonly ViewConfig _viewConfig;
        private readonly BookSettingConfig _bookSettingConfig;

        public ViewPropertyControl(ViewConfig viewConfig, BookSettingConfig bookSettingConfig)
        {
            _viewConfig = viewConfig;
            _bookSettingConfig = bookSettingConfig;

            _bookSettingConfig.SubscribePropertyChanged(nameof(BookSettingConfig.AutoRotate), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsAutoRotateLeft));
                RaisePropertyChanged(nameof(IsAutoRotateRight));
                RaisePropertyChanged(nameof(IsAutoRotateForcedLeft));
                RaisePropertyChanged(nameof(IsAutoRotateForcedRight));
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

        public bool IsAutoRotateForcedLeft
        {
            get => GetAutoRotateForcedLeft();
            set => SetAutoRotateForcedLeft(value);
        }

        public bool IsAutoRotateForcedRight
        {
            get => GetAutoRotateForcedRight();
            set => SetAutoRotateForcedRight(value);
        }


        public bool GetAutoRotateLeft()
        {
            return _bookSettingConfig.AutoRotate == AutoRotateType.Left;
        }

        public bool GetAutoRotateRight()
        {
            return _bookSettingConfig.AutoRotate == AutoRotateType.Right;
        }

        public bool GetAutoRotateForcedLeft()
        {
            return _bookSettingConfig.AutoRotate == AutoRotateType.ForcedLeft;
        }

        public bool GetAutoRotateForcedRight()
        {
            return _bookSettingConfig.AutoRotate == AutoRotateType.ForcedRight;
        }


        public void SetAutoRotateLeft(bool flag)
        {
            _bookSettingConfig.AutoRotate = flag ? AutoRotateType.Left : AutoRotateType.None;
        }

        public void SetAutoRotateRight(bool flag)
        {
            _bookSettingConfig.AutoRotate = flag ? AutoRotateType.Right : AutoRotateType.None;
        }

        public void SetAutoRotateForcedLeft(bool flag)
        {
            _bookSettingConfig.AutoRotate = flag ? AutoRotateType.ForcedLeft : AutoRotateType.None;
        }

        public void SetAutoRotateForcedRight(bool flag)
        {
            _bookSettingConfig.AutoRotate = flag ? AutoRotateType.ForcedRight : AutoRotateType.None;
        }

        public void ToggleAutoRotateLeft()
        {
            _bookSettingConfig.AutoRotate = _bookSettingConfig.AutoRotate != AutoRotateType.Left ? AutoRotateType.Left : AutoRotateType.None;
        }

        public void ToggleAutoRotateRight()
        {
            _bookSettingConfig.AutoRotate = _bookSettingConfig.AutoRotate != AutoRotateType.Right ? AutoRotateType.Right : AutoRotateType.None;
        }

        public void ToggleAutoRotateForcedLeft()
        {
            _bookSettingConfig.AutoRotate = _bookSettingConfig.AutoRotate != AutoRotateType.ForcedLeft ? AutoRotateType.ForcedLeft : AutoRotateType.None;
        }

        public void ToggleAutoRotateForcedRight()
        {
            _bookSettingConfig.AutoRotate = _bookSettingConfig.AutoRotate != AutoRotateType.ForcedRight ? AutoRotateType.ForcedRight : AutoRotateType.None;
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

        public void SetStretchMode(PageStretchMode mode, bool isToggle, bool force)
        {
            _viewConfig.SetStretchMode(GetFixedStretchMode(mode, isToggle), force);
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