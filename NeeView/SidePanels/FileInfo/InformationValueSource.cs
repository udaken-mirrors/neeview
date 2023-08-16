using NeeView.Media.Imaging.Metadata;

namespace NeeView
{
    public class InformationValueSource
    {
        public InformationValueSource(Page? page, PictureInfo? pictureInfo, BitmapMetadataDatabase? metadata)
        {
            Page = page;
            PictureInfo = pictureInfo;
            Metadata = metadata;
        }

        public Page? Page { get; }
        public PictureInfo? PictureInfo { get; }
        public BitmapMetadataDatabase? Metadata { get; }
    }
}
