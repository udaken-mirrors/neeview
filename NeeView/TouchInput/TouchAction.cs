﻿using System.Collections.Generic;

namespace NeeView
{
    // タッチアクション
    public enum TouchAction
    {
        [AliasName("なし")]
        None,

        [AliasName("タッチビュー操作")]
        Drag,

        [AliasName("マウスドラッグ操作")]
        MouseDrag,

        [AliasName("ジェスチャー")]
        Gesture,
    }

    //
    public static class TouchActionExtensions
    {
        public static Dictionary<TouchAction, string> TouchActionTips = new Dictionary<TouchAction, string>
        {
            [TouchAction.None] = null,
            [TouchAction.Drag] = "タッチによるビュー操作です",
            [TouchAction.MouseDrag] = "マウスの左ボタンドラッグと同じビュー操作です",
            [TouchAction.Gesture] = "ジェスチャー入力です"
        };
    }
}
