using NeeView.Windows.Property;

namespace NeeView
{
    public class ScriptCommandParameter : CommandParameter
    {
        private string? _argument;
        private string? _checkFlagKey;

        [PropertyMember]
        public string? Argument
        {
            get { return _argument; }
            set { SetProperty(ref _argument, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        [PropertyMember]

        public string? CheckFlagKey
        {
            get { return _checkFlagKey; }
            set { SetProperty(ref _checkFlagKey, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

    }

}
