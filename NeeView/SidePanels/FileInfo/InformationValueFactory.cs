using NeeLaboratory.ComponentModel;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class InformationValueFactory
    {
        private readonly Page? _page;

        public InformationValueFactory(Page? page)
        {
            _page = page;
        }

        public object? Create(InformationKey key)
        {
            if (_page is null) return null;

            return PageMetadataTools.GetValue(_page, key);
        }

        public Dictionary<string, object?> GetExtraMap()
        {
            return _page?.Content.PictureInfo?.Metadata?.ExtraMap ?? new();
        }

    }
}
