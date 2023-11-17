using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PdfPageThumbnail : ImagePageThumbnail
    {
        public PdfPageThumbnail(PdfPageContent content) : base(content, new PdfPictureSource(content.ArchiveEntry, content.PictureInfo))
        {
        }
    }
}
