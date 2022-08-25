using System.Runtime.Serialization;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// ページが終わったときのアクション
    /// </summary>
    [DataContract]
    public enum PageEndAction
    {
        [EnumMember]
        [AliasName]
        None,

        [EnumMember(Value = "NextFolder")]
        [AliasName]
        NextBook,

        [EnumMember]
        [AliasName]
        Loop,

        [EnumMember]
        [AliasName]
        Dialog,
    }
}

