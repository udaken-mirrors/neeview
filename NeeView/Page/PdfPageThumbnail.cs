using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PdfPageThumbnail : PicturePageThumbnail
    {
        public PdfPageThumbnail(PdfPageContent content) : base(content, new PdfPictureSource(content))
        {
        }
    }
}
