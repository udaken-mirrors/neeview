using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PdfContentLoader : BitmapContentLoader
    {
        public PdfContentLoader(PdfContent content) : base(content)
        {
        }

        public override async Task LoadContentAsync(CancellationToken token)
        {
            // NOTE: BitmapSourceは表示直前で生成する
            await LoadContentAsyncTemplate(null, token);
        }
    }
}
