using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NeeView
{
    public class PdfPageContent : PageContent
    {
        private readonly PdfArchiver _pdfArchive;

        public PdfPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
            _pdfArchive = archiveEntry.Archiver as PdfArchiver ?? throw new InvalidOperationException();
        }


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            await Task.CompletedTask;

            try
            {
                token.ThrowIfCancellationRequested();
                var pictureInfo = CreatePictureInfo(token);
                return PageSource.Create(Entry, pictureInfo);
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }

        private PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (PictureInfo != null) return PictureInfo;

            var pictureInfo = new PictureInfo();
            var originalSize = _pdfArchive.GetSourceSize(Entry); // TODO: async
            pictureInfo.OriginalSize = originalSize;
            var maxSize = Config.Current.Performance.MaximumSize;
            var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : originalSize;
            pictureInfo.Size = size;
            pictureInfo.BitsPerPixel = 32;
            pictureInfo.Decoder = _pdfArchive.ToString();
            return pictureInfo;
        }
    }
}