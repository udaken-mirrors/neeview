using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class GifMetadataAccessor : BitmapMetadataAccessor
    {
        private readonly BitmapMetadata _meta;
        private readonly List<string> _comments = new();

        public GifMetadataAccessor(BitmapMetadata meta)
        {
            _meta = meta ?? throw new ArgumentNullException(nameof(meta));
            Debug.Assert(_meta.Format == "gif");

            // commentext  map
            foreach (var key in meta.Where(e => e.EndsWith("commentext", StringComparison.Ordinal)))
            {
                if (meta.GetQuery(key) is BitmapMetadata commentextMeta)
                {
                    var text = commentextMeta.GetQuery("/TextEntry") as string;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        _comments.Add(text);
                    }
                }
            }
        }

        public override string GetFormat()
        {
            return _meta.Format.ToUpperInvariant();
        }

        public override object? GetValue(BitmapMetadataKey key)
        {
            return key switch
            {
                BitmapMetadataKey.Comments => string.Join(System.Environment.NewLine, _comments),
                _ => null,
            };
        }
    }

}
