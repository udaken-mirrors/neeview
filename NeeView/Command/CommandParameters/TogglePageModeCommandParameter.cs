using NeeView.Windows.Property;

namespace NeeView
{
    public class TogglePageModeCommandParameter : CommandParameter
    {
        private bool _isLoop;

        // ループ
        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }
    }

}
