using System;

namespace NeeView
{
    // 複数ページのときの動作
    public enum MultiPagePolicy
    {
        [AliasName]
        Once,

        [AliasName]
        All,

        [AliasName]
        AllLeftToRight,

        [Obsolete("no used")] // ver.37
        [AliasName(IsVisible = false)]
        Twice = All,
    };
}
