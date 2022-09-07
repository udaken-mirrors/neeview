using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeeView.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionValuesAttribute : OptionBaseAttribute
    {
    }


    public class OptionValuesElement
    {
        private readonly PropertyInfo _info;

        public OptionValuesElement(PropertyInfo info)
        {
            _info = info;

            if (info.PropertyType != typeof(List<string>)) throw new InvalidOperationException("The property of the OptionValues attribute must be of type List<string>");
        }

        public void SetValues(object source, List<string> values)
        {
            _info.SetValue(source, values);
        }
    }


}
