namespace NeeView
{
    public static class PdfArchiveFactory
    {
        public static PdfArchive Create(string path, ArchiveEntry? source)
        {
#if USE_WINRT
            return PdfArchiveConfig.GetPdfRenderer() switch
            {
                PdfRenderer.WinRT
                    => new PdfWinRTArchive(path, source),
                _
                    => new PdfPdfiumArchive(path, source),
            };
#else
            return new PdfPdfiumArchive(path, source);
#endif
        }
    }
}
