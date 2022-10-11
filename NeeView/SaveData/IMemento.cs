using System;

namespace NeeView
{
    /// <summary>
    /// Memento データであることを示す属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MementoAttribute : Attribute
    {
    }
}
