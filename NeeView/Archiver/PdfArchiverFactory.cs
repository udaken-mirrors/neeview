namespace NeeView
{
    public static class PdfArchiverFactory
    {
        public static PdfArchiver Create(string path, ArchiveEntry? source)
        {
            return PdfArchiveConfig.GetPdfRenderer() switch
            {
                PdfRenderer.WinRT
                    => new PdfWinRTArchiver(path, source),
                _
                    => new PdfPdfiumArchiver(path, source),
            };
        }
    }
}
