using System;

namespace NeeView
{
    /// <summary>
    /// ドキュメント候補属性
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class DocumentableAttribute : Attribute
    {
        private string? _name;

        public DocumentableAttribute()
        {
        }

        /// <summary>
        /// 名前。nullの場合はタイプ名がそのまま使われる
        /// </summary>
        public string? Name
        {
            get { return _name ?? DocumentType?.Name; }
            set { _name = value; }
        }

        /// <summary>
        /// 型。stringであるが、実態はEnumである場合等に指定する
        /// </summary>
        public Type? DocumentType { get; set; }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentableBaseClassAttribute : Attribute
    {
        public Type BaseClass { get; }

        public DocumentableBaseClassAttribute(Type baseClass)
        {
            BaseClass = baseClass;
        }
    }
}
