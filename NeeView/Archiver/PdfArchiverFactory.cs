namespace NeeView
{
    public static class PdfArchiverFactory
    {
        public static PdfArchiver Create(string path, ArchiveEntry? source)
        {
#if USE_WINRT
            return PdfArchiveConfig.GetPdfRenderer() switch
            {
                PdfRenderer.WinRT
                    => new PdfWinRTArchiver(path, source),
                _
                    => new PdfPdfiumArchiver(path, source),
            };
#else
            return new PdfPdfiumArchiver(path, source);
#endif
        }
    }
}
