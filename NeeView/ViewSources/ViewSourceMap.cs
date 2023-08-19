using System.Collections.Generic;
using System.Diagnostics;

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

        public ViewSource Get(Page page, PagePart pagePart)
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

        public void Clear()
        {
            _map.Clear();
        }
    }


    public record struct ViewSourceKey(Page Page, PagePart PagePart);
}
