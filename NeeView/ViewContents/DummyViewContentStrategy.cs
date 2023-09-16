using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class DummyViewContentStrategy : IViewContentStrategy
    {
        public void Dispose()
        {
        }

        public void OnSourceChanged()
        {
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            var grid = new Grid();
            grid.SetBinding(Grid.BackgroundProperty, new Binding(nameof(BookConfig.DummyPageColor)) { Source = Config.Current.Book, Converter = new ColorToBrushConverter() });
            return grid;
        }

    }
}
