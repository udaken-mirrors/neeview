using NeeView.Data;
using System;

namespace NeeView
{
    public class DragActionParameterSource
    {
        private DragActionParameter? _parameter;
        private Type _type;

#if false
        public DragActionParameterSource()
        {
        }
#endif

        public DragActionParameterSource(Type defaultType)
        {
            if (defaultType == null) throw new ArgumentNullException(nameof(defaultType));

            _type = defaultType;
        }


        public DragActionParameter? GetRaw()
        {
            return _parameter;
        }

        public DragActionParameter GetDefault()
        {
            var instance = Activator.CreateInstance(_type) as DragActionParameter;
            if (instance is null) throw new InvalidOperationException();
            return instance;
        }

        public DragActionParameter Get()
        {
            if (_parameter is null)
            {
                _parameter = GetDefault();
            }

            return _parameter;
        }

        public void Set(DragActionParameter? value)
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
    }
}
