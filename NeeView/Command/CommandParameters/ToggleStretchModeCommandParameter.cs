using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    public class ToggleStretchModeCommandParameter : CommandParameter
    {
        private bool _isLoop = true;
        private bool _isEnableNone = true;
        private bool _isEnableUniform = true;
        private bool _isEnableUniformToFill = true;
        private bool _isEnableUniformToSize = true;
        private bool _isEnableUniformToVertical = true;
        private bool _isEnableUniformToHorizontal = true;

        // ループ
        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        // 表示名
        [PropertyMember]
        public bool IsEnableNone
        {
            get => _isEnableNone;
            set => SetProperty(ref _isEnableNone, value);
        }

        [PropertyMember]
        public bool IsEnableUniform
        {
            get => _isEnableUniform;
            set => SetProperty(ref _isEnableUniform, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToFill
        {
            get => _isEnableUniformToFill;
            set => SetProperty(ref _isEnableUniformToFill, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToSize
        {
            get => _isEnableUniformToSize;
            set => SetProperty(ref _isEnableUniformToSize, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToVertical
        {
            get => _isEnableUniformToVertical;
            set => SetProperty(ref _isEnableUniformToVertical, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToHorizontal
        {
            get => _isEnableUniformToHorizontal;
            set => SetProperty(ref _isEnableUniformToHorizontal, value);
        }


        public IReadOnlyDictionary<PageStretchMode, bool> GetStretchModeDictionary()
        {
            return new Dictionary<PageStretchMode, bool>()
            {
                [PageStretchMode.None] = IsEnableNone,
                [PageStretchMode.Uniform] = IsEnableUniform,
                [PageStretchMode.UniformToFill] = IsEnableUniformToFill,
                [PageStretchMode.UniformToSize] = IsEnableUniformToSize,
                [PageStretchMode.UniformToVertical] = IsEnableUniformToVertical,
                [PageStretchMode.UniformToHorizontal] = IsEnableUniformToHorizontal,
            };
        }
    }

}
