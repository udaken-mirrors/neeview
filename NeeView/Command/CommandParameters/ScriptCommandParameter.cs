using NeeView.Windows.Property;

namespace NeeView
{
    public class ScriptCommandParameter : CommandParameter
    {
        private string? _argument;

        [PropertyMember]
        public string? Argument
        {
            get { return _argument; }
            set { SetProperty(ref _argument, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }
    }

}
