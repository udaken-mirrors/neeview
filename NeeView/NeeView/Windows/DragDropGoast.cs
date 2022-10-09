// from https://github.com/takanemu/WPFDragAndDropSample

using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace NeeView.Windows
{
    /// <summary>
    /// ドラッグドロップ用ゴースト
    /// </summary>
    public class DragDropGoast
    {
        private UIElement? _element;
        private AdornerLayer? _layer;
        private DragAdorner? _goast;

        /// <summary>
        /// 専用ビジュアルをゴーストにする
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pos"></param>
        /// <param name="visual"></param>
        public void Attach(UIElement element, Point pos, UIElement visual)
        {
            if (_layer != null)
            {
                Detach();
            }

            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (window == null)
            {
                return;
            }

            if (window.Content is UIElement root)
            {
                _element = element;
                _layer = AdornerLayer.GetAdornerLayer(root);
                _goast = new DragAdorner(root, visual, 1.0, 0, pos);
                _layer.Add(_goast);
            }
        }

        /// <summary>
        /// ドラッグビジュアルをそのままゴーストにする
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pos"></param>
        public void Attach(UIElement element, Point pos)
        {
            if (_layer != null)
            {
                Detach();
            }

            var window = Window.GetWindow(element) ?? Application.Current.MainWindow;
            if (window.Content is UIElement root)
            {
                _element = element;
                _layer = AdornerLayer.GetAdornerLayer(root);
                _goast = new DragAdorner(root, _element, 0.5, 0, pos);
                _layer.Add(_goast);
            }
        }


        public void Detach()
        {
            _layer?.Remove(_goast);
            _layer = null;
            _goast = null;
            _element = null;
        }

        public void QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            if (_goast == null || _element == null)
            {
                return;
            }

            try
            {
                var point = CursorInfo.GetNowPosition(_element);
                if (double.IsNaN(point.X))
                {
                    e.Action = System.Windows.DragAction.Cancel;
                    e.Handled = true;
                    return;
                }

                _goast.LeftOffset = point.X;
                _goast.TopOffset = point.Y;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
