using System.Windows;

namespace NeeView.Maths
{
    /// <summary>
    /// fron NeeView.DragTransformControl
    /// スクロールロック
    /// </summary>
    public class ScrollLock
    {
        private bool _lockMoveX = true;
        private bool _lockMoveY = true;


        public bool IsXLocked => _lockMoveX;
        public bool IsYLocked => _lockMoveY;


        public void Update(Rect contentRect, Rect viewRect)
        {
            var area = new DragArea(viewRect, contentRect);

            double margin = 1.1;

            if (_lockMoveX)
            {
                if (area.Over.Left + margin < 0 || area.Over.Right - margin > 0)
                {
                    _lockMoveX = false;
                }
            }
            if (_lockMoveY)
            {
                if (area.Over.Top + margin < 0 || area.Over.Bottom - margin > 0)
                {
                    _lockMoveY = false;
                }
            }
        }

        public void Lock()
        {
            _lockMoveX = true;
            _lockMoveY = true;
        }

        public void Unlock()
        {
            _lockMoveX = false;
            _lockMoveY = false;
        }

        /// <summary>
        /// 移動ベクトルに制限を適用した値を返す
        /// </summary>
        public Vector Limit(Vector delta)
        {
            return new Vector(_lockMoveX ? 0.0 : delta.X, _lockMoveY ? 0.0 : delta.Y);
        }
    }
}