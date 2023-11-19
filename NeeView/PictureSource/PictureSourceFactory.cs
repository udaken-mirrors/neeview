using System;
using System.Diagnostics;

namespace NeeView
{
    public static class PictureSourceFactory
    {
        public static IPictureSource Create(PageContent content)
        {
            Debug.Assert(content.PictureInfo is not null);
            switch (content)
            {
                case BitmapPageContent:
                    return new BitmapPictureSource(content.ArchiveEntry, content.PictureInfo);
                case SvgPageContent:
                    return new SvgPictureSource(content.ArchiveEntry, content.PictureInfo);
                case PdfPageContent:
                    return new PdfPictureSource(content.ArchiveEntry, content.PictureInfo);
                default:
                    throw new NotSupportedException();
            }
        }
    }

}
