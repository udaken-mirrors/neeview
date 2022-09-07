using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // マウスジェスチャー 方向
    public enum MouseGestureDirection
    {
        None,
        Up,
        Right,
        Down,
        Left,
        Click,
    }

    /// <summary>
    /// マウスジェスチャー シーケンス
    /// </summary>
    public class MouseGestureSequence : ObservableCollection<MouseGestureDirection>
    {
        private static readonly Dictionary<MouseGestureDirection, string> _dispStrings = new()
        {
            [MouseGestureDirection.None] = "",
            [MouseGestureDirection.Up] = "↑",
            [MouseGestureDirection.Right] = "→",
            [MouseGestureDirection.Down] = "↓",
            [MouseGestureDirection.Left] = "←",
            [MouseGestureDirection.Click] = "Click"
        };


        private static readonly Dictionary<char, MouseGestureDirection> _table = new()
        {
            ['U'] = MouseGestureDirection.Up,
            ['R'] = MouseGestureDirection.Right,
            ['D'] = MouseGestureDirection.Down,
            ['L'] = MouseGestureDirection.Left,
            ['C'] = MouseGestureDirection.Click,
        };


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseGestureSequence()
        {
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="gestureText">記録用文字列</param>
        public MouseGestureSequence(string gestureText)
        {
            if (!string.IsNullOrEmpty(gestureText))
            {
                foreach (char c in gestureText)
                {
                    if (_table.TryGetValue(c, out MouseGestureDirection direction))
                    {
                        this.Add(direction);
                    }
                }
            }
        }



        // 記録用文字列に変換(U,D,L,R,Cの組み合わせ)
        public override string ToString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += e.ToString()[0];
            }

            return gestureText;
        }


        // 表示文字列に変換(矢印の組み合わせ)
        public string ToDispString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += _dispStrings[e];
            }

            return gestureText;
        }
    }
}
