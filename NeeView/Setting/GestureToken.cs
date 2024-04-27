using System.Collections.Generic;

namespace NeeView.Setting
{
    public class GestureToken
    {
        // 競合しているコマンド群
        public List<string>? Conflicts { get; set; }

        // 競合メッセージ
        public string? OverlapsText { get; set; }

        // 競合している？
        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }
}
