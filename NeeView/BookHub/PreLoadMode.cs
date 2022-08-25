using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// 先読みモード
    /// </summary>
    [Obsolete]
    public enum PreLoadMode
    {
        None,
        AutoPreLoad,
        PreLoad,
        PreLoadNoUnload,
    }
}

