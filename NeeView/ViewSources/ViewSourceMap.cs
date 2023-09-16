using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public class ViewSourceMap
    {
        private readonly Dictionary<ViewSourceKey, ViewSource> _map = new();
        private readonly object _lock = new();
        private readonly BookMemoryService _bookMemoryService;

        public ViewSourceMap(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
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

                    var viewSource = new ViewSource(page.Content, _bookMemoryService);
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
