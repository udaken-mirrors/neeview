using System.Collections.Generic;
using System.Globalization;

namespace NeeView
{
    public class PlaylistListBoxItemCollection : List<PlaylistItem>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), nameof(PlaylistListBoxItemCollection));

        public PlaylistListBoxItemCollection()
        {
        }

        public PlaylistListBoxItemCollection(IEnumerable<PlaylistItem> collection) : base(collection)
        {
        }
    }
}
