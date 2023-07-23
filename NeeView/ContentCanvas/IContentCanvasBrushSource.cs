using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public interface IContentCanvasBrushSource
    {
        public event EventHandler? ContentChanged;
        public event EventHandler? DpiChanged;
        public DpiScale Dpi { get; }
        public Color GetContentColor();
    }
}
