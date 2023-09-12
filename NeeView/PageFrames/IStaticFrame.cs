using System.ComponentModel;
using System.Windows;

namespace NeeView.PageFrames
{
    public interface IStaticFrame : INotifyPropertyChanged
    {
        public bool IsStaticFrame { get; }
        public Size CanvasSize { get; }
        public DpiScale DpiScale { get; }
    }
}
