using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Documents;

namespace NeeView
{
    public class ViewSourceMap
    {
        private Dictionary<ViewSourceKey, ViewSource> _map = new();
        private ViewSourceFactory _factory;

        public ViewSourceMap(BookMemoryService bookMemoryService)
        {
            _factory = new ViewSourceFactory(bookMemoryService);
        }

        public ViewSource Get(PageRange pageRange, IPageContent pageContent)
        {
            var key = new ViewSourceKey(pageRange, pageContent);
            if (_map.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                //Debug.WriteLine($"ViewSourceMap.Create: {key}");

                var viewSource = _factory.Create(pageContent);
                _map.Add(key, viewSource);
                return viewSource;
            }
        }

        public void Clear()
        {
            _map.Clear();
        }
    }


    public record struct ViewSourceKey(PageRange PageRange, IPageContent PageContent);
}
