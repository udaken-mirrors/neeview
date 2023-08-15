using System;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView.Maths
{
    /// <summary>
    /// from NeeView.DragTransformControl
    /// スクロールエリア制限
    /// TODO: DragArea に統合可能？
    /// </summary>
    public class ScrollAreaLimit
    {
        private readonly Rect _contentRect;
        private readonly Rect _viewRect;

        public ScrollAreaLimit(Rect contentRect, Rect viewRect)
        {
            _contentRect = contentRect;
            _viewRect = viewRect;
        }

        // 移動量限界計算
        public Vector GetLimitViewMove(Vector delta)
        {
            var margineX = _contentRect.Width < _viewRect.Width ? _viewRect.Width - _contentRect.Width : 0;
            var margineY = _contentRect.Height < _viewRect.Height ? _viewRect.Height - _contentRect.Height : 0;

            if (delta.X < 0 && _viewRect.Left + delta.X < _contentRect.Left - margineX)
            {
                delta.X = Math.Min(_contentRect.Left - margineX - _viewRect.Left, 0.0);
            }
            else if (delta.X > 0 && _viewRect.Right + delta.X > _contentRect.Right + margineX)
            {
                delta.X = Math.Max(_contentRect.Right + margineX - _viewRect.Right, 0.0);
            }

            if (delta.Y < 0 && _viewRect.Top + delta.Y < _contentRect.Top - margineY)
            {
                delta.Y = Math.Min(_contentRect.Top - margineY - _viewRect.Top, 0.0);
            }
            else if (delta.Y > 0 && _viewRect.Bottom + delta.Y > _contentRect.Bottom + margineY)
            {
                delta.Y = Math.Max(_contentRect.Bottom + margineY - _viewRect.Bottom, 0.0);
            }

            return delta;
        }

        public Vector GetLimitContentMove(Vector delta)
        {
            var margineX = _contentRect.Width < _viewRect.Width ? _viewRect.Width - _contentRect.Width : 0;
            var margineY = _contentRect.Height < _viewRect.Height ? _viewRect.Height - _contentRect.Height : 0;

            if (delta.X < 0 && _contentRect.Right + delta.X < _viewRect.Right - margineX)
            {
                delta.X = Math.Min(_viewRect.Right - margineX - _contentRect.Right, 0.0);
            }
            else if (delta.X > 0 && _contentRect.Left + delta.X > _viewRect.Left + margineX)
            {
                delta.X = Math.Max(_viewRect.Left + margineX - _contentRect.Left, 0.0);
            }

            if (delta.Y < 0 && _contentRect.Bottom + delta.Y < _viewRect.Bottom - margineY)
            {
                delta.Y = Math.Min(_viewRect.Bottom - margineY - _contentRect.Bottom, 0.0);
            }
            else if (delta.Y > 0 && _contentRect.Top + delta.Y > _viewRect.Top + margineY)
            {
                delta.Y = Math.Max(_viewRect.Top + margineY - _contentRect.Top, 0.0);
            }

            return delta;
        }


        /// <summary>
        ///  エリアサイズ内に座標を収める
        /// </summary>
        /// <param name="centered">範囲内に収まるときは中央に配置</param>
        /// <returns>補正された座標</returns>
        public Point SnapView(bool centered)
        {
            const double margin = 1.0;

            var pos = _contentRect.Center();

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - _contentRect.Width * 0.5 + _viewRect.Width * 0.5,
                Y = pos.Y - _contentRect.Height * 0.5 + _viewRect.Height * 0.5,
                Width = _contentRect.Width,
                Height = _contentRect.Height,
            };

            var minX = _viewRect.Width * -0.5 + rect.Width * 0.5;
            var maxX = minX + _viewRect.Width - rect.Width;

            if (rect.Width <= _viewRect.Width + margin)
            {
                if (centered)
                {
                    pos.X = 0.0;
                }
                else if (rect.Left < 0)
                {
                    pos.X = minX;
                }
                else if (rect.Right > _viewRect.Width)
                {
                    pos.X = maxX;
                }
            }
            else
            {
                if (rect.Left > 0)
                {
                    pos.X -= rect.Left;
                }
                else if (rect.Right < _viewRect.Width)
                {
                    pos.X += _viewRect.Width - rect.Right;
                }
            }

            var minY = _viewRect.Height * -0.5 + rect.Height * 0.5;
            var maxY = minY + _viewRect.Height - rect.Height;

            if (rect.Height <= _viewRect.Height + margin)
            {
                if (centered)
                {
                    pos.Y = 0.0;
                }
                else if (rect.Top < 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom > _viewRect.Height)
                {
                    pos.Y = maxY;
                }
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom < _viewRect.Height)
                {
                    pos.Y = maxY;
                }
            }

            return pos;
        }
    }
}