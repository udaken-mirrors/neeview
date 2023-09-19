using System;

namespace NeeView
{
    /// <summary>
    /// 代替案を示す属性。Obsolete属性とともに使用する
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public class AlternativeAttribute : Attribute
    {
        public AlternativeAttribute(string? alternative, int version)
            : this(alternative, version, ScriptErrorLevel.Error)
        {
        }

        public AlternativeAttribute(string? alternative, int version, ScriptErrorLevel errorLevel)
        {
            Alternative = alternative;
            Version = version;
            ErrorLevel = errorLevel;
        }


        /// <summary>
        /// 代替案
        /// </summary>
        public string? Alternative { get; }

        /// <summary>
        /// 完全な形でAlternativeが記述されている。
        /// </summary>
        /// <remarks>
        /// falseの場合はAlternativeは同じクラス内でのメンバーを示している
        /// </remarks>
        public bool IsFullName { get; set; }

        /// <summary>
        /// 適用されたバージョン
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// エラーレベル
        /// </summary>
        public ScriptErrorLevel ErrorLevel { get; set; } = ScriptErrorLevel.Error;
    }
}
