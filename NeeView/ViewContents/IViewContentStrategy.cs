using System;
using System.Windows;

namespace NeeView
{
    public interface IViewContentStrategy : IDisposable, IHasImageSource, IHasScalingMode
    {
        FrameworkElement CreateLoadedContent(object data);
        void OnSourceChanged();
    }
}
