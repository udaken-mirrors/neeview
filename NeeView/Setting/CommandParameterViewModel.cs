using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    public class CommandParameterViewModel : BindableBase
    {
        private readonly IDictionary<string, CommandElement> _commandMap;
        private readonly string _key;

        private readonly CommandParameter? _defaultParameter;
        private PropertyDocument? _propertyDocument;


        public PropertyDocument? PropertyDocument
        {
            get { return _propertyDocument; }
            set { if (_propertyDocument != value) { _propertyDocument = value; RaisePropertyChanged(); } }
        }

        public string? Note { get; private set; }



        public CommandParameterViewModel(IDictionary<string, CommandElement> commandMap, string key)
        {
            _commandMap = commandMap;
            _key = key;

            var commandElement = CommandTable.Current.GetElement(_key);
            if (commandElement.Share != null)
            {
                _key = commandElement.Share.Name;
                this.Note = string.Format(Properties.TextResources.GetString("CommandParameter.Share"), CommandTable.Current.GetElement(_key).Text);
            }

            _defaultParameter = commandElement.ParameterSource?.GetDefault();
            if (_defaultParameter == null)
            {
                return;
            }

            var parameter = (CommandParameter?)(_commandMap[_key].Parameter ?? _defaultParameter)?.Clone();
            if (parameter is not null)
            {
                _propertyDocument = new PropertyDocument(parameter);
            }
        }

        public void Flush()
        {
            if (_propertyDocument != null)
            {
                _commandMap[_key].Parameter = (CommandParameter?)_propertyDocument.Source;
            }
        }

        public void Reset()
        {
            if (_propertyDocument != null && _defaultParameter != null)
            {
                _propertyDocument.Set(_defaultParameter);
                RaisePropertyChanged(nameof(PropertyDocument));
            }
        }
    }
}
