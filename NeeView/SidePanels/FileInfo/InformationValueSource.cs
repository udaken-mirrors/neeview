using NeeView.Media.Imaging.Metadata;

namespace NeeView
{
    public class InformationValueSource
    {
        public InformationValueSource(Page? page, BitmapPageContent? bitmapContent, BitmapMetadataDatabase? metadata)
        {
            Page = page;
            BitmapContent = bitmapContent;
            Metadata = metadata;
        }

        public Page? Page { get; }
        public BitmapPageContent? BitmapContent { get; }
        public BitmapMetadataDatabase? Metadata { get; }
    }
}
