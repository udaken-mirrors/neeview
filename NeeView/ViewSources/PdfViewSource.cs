using System;
using System.Windows.Documents;

namespace NeeView
{

    public class PdfViewSource : PictureViewSource
    {
        public PdfViewSource(PdfPageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, new PdfPictureSource(pageContent), bookMemoryService)
        {
        }
    }
}