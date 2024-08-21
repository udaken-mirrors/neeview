using System;
using System.Reflection;

namespace NeeView
{
    public abstract class ScriptClassMemberInfo
    {
        private readonly MemberInfo _memberInfo;
        private readonly Type _type;
        private readonly ObsoleteAttribute? _obsoleteAttribute;
        private readonly AlternativeAttribute? _alternativeAttribute;

        public ScriptClassMemberInfo(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
            _type = _memberInfo.ReflectedType ?? throw new ArgumentException("ReflectedType is required for memberInfo.", nameof(memberInfo));

            _obsoleteAttribute = _memberInfo.GetCustomAttribute<ObsoleteAttribute>();
            _alternativeAttribute = _memberInfo.GetCustomAttribute<AlternativeAttribute>();
        }

        public MemberInfo MemberInfo => _memberInfo;
        public ObsoleteAttribute? ObsoleteAttribute => _obsoleteAttribute;
        public AlternativeAttribute? AlternativeAttribute => _alternativeAttribute;

        public virtual string Name => _type.Name + "." + _memberInfo.Name;
        public bool HasObsolete => _obsoleteAttribute != null;
        public bool HasAlternative => _alternativeAttribute != null;

        public override string ToString()
        {
            return Name;
        }
    }


    public class ScriptClassPropertyInfo : ScriptClassMemberInfo
    {
        public ScriptClassPropertyInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
    }


    public class ScriptClassMethodInfo : ScriptClassMemberInfo
    {
        public ScriptClassMethodInfo(MethodInfo methodInfo) : base(methodInfo)
        {
            MethodInfo = methodInfo;
        }

        public MethodInfo MethodInfo { get; }

        public override string Name => base.Name + "()";
    }
}
