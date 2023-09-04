using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public class ViewSourceMap
    {
        private Dictionary<ViewSourceKey, ViewSource> _map = new();
        private ViewSourceFactory _factory;
        private object _lock = new();

        public ViewSourceMap(BookMemoryService bookMemoryService)
        {
            _factory = new ViewSourceFactory(bookMemoryService);
        }

        public ViewSource Get(Page page, PagePart pagePart)
        {
            lock (_lock)
            {
                var key = new ViewSourceKey(page, pagePart);
                if (_map.TryGetValue(key, out var value))
                {
                    return value;
                }
                else
                {
                    //Debug.WriteLine($"ViewSourceMap.Create: {key}");

                    var viewSource = _factory.Create(page.Content);
                    _map.Add(key, viewSource);
                    return viewSource;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
            }
        }
    }


    public record struct ViewSourceKey(Page Page, PagePart PagePart);
}
