using System.Collections.Generic;

namespace NeeView.Media.Imaging.Metadata
{
    public abstract class BitmapMetadataAccessor
    {
        public abstract string GetFormat();

        public abstract object? GetValue(BitmapMetadataKey key);

        public virtual Dictionary<string, object?> GetExtraValues()
        {
            return new();
        }
    }
}
