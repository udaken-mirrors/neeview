using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public class ScriptClassInfo
    {
        private readonly List<ScriptClassMemberInfo> _members;

        public ScriptClassInfo(Type type)
        {
            Debug.Assert(type.IsClass);

            _members = Collect(type);
        }

        public List<ScriptClassMemberInfo> Members => _members;


        private static List<ScriptClassMemberInfo> Collect(Type type)
        {
            Debug.Assert(type.IsClass);

            var members = new List<ScriptClassMemberInfo>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                members.Add(new ScriptClassPropertyInfo(property));
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                members.Add(new ScriptClassMethodInfo(method));
            }

            return members;
        }

        static bool IsDocumentable(MemberInfo info)
        {
            return info.GetCustomAttribute<DocumentableAttribute>() != null;
        }
    }
}
