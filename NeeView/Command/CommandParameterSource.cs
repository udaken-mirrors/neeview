using NeeView.Data;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class CommandParameterSource
    {
        private CommandParameter? _parameter;
        private Type _type;

#if false
        public CommandParameterSource()
        {
        }
#endif

        // TODO: 型を直接指定するように
        public CommandParameterSource(CommandParameter defaultParameter)
        {
            if (defaultParameter == null) throw new ArgumentNullException(nameof(defaultParameter));

            _type = defaultParameter.GetType(); // ##
        }


        public CommandParameter? GetRaw()
        {
            return _parameter;
        }

        public CommandParameter GetDefault()
        {
            var parameter = Activator.CreateInstance(_type) as CommandParameter;
            if (parameter is null) throw new InvalidOperationException();
            return parameter;
        }

        public CommandParameter Get()
        {
            if (_parameter is null)
            {
                _parameter = GetDefault();
            }

            return _parameter;
        }

        public void Set(CommandParameter? value)
        {
            if (value != null && value.GetType() != _type)
            {
                throw new ArgumentException($"{_type} is required: not {value.GetType()}");
            }

            _parameter = value;
        }

        public bool EqualsDefault()
        {
            return GetDefault().MemberwiseEquals(_parameter);
        }


        [Obsolete]
        public string Store()
        {
            if (_parameter is null) return "";
            return Json.Serialize(_parameter, _type);
        }

        [Obsolete]
        public void Restore(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                _parameter = null;
            }
            else
            { 
                _parameter = (CommandParameter?)Json.Deserialize(json, _type);
            }
        }
    }
}
