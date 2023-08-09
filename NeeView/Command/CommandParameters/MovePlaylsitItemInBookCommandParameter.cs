using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// プレイリスト項目移動用パラメータ
    /// </summary>
    public class MovePlaylsitItemInBookCommandParameter : CommandParameter
    {
        private bool _isLoop;
        private bool _isIncludeTerminal;

        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        [PropertyMember]
        public bool IsIncludeTerminal
        {
            get => _isIncludeTerminal;
            set => SetProperty(ref _isIncludeTerminal, value);
        }
    }
}
