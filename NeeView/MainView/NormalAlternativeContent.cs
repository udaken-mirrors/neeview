using System.Windows;

namespace NeeView
{
    public class NormalAlternativeContent : IDisposableContent
    {
        private readonly FrameworkElement _element;

        public NormalAlternativeContent(FrameworkElement element)
        {
            _element = element;
        }

        public object? Content => _element;

        public void Dispose()
        {
        }
    }
}
