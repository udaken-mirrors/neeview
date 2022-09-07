using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// ViewContent Factory
    /// </summary>
    public class ViewContentFactory
    {
        public static ViewContent Create(MainViewComponent viewComponent, ViewContentSource source, ViewContent oldViewContent)
        {
            ViewContent viewContent = source.GetContentType() switch
            {
                ViewContentType.Dummy => DummyViewContent.Create(viewComponent, source),
                ViewContentType.Message => MessageViewContent.Create(viewComponent, source),
                ViewContentType.Reserve => ReserveViewContent.Create(viewComponent, source, oldViewContent),
                ViewContentType.Bitmap => BitmapViewContent.Create(viewComponent, source),
                ViewContentType.Anime => AnimatedViewContent.Create(viewComponent, source),
                ViewContentType.Media => MediaViewContent.Create(viewComponent, source),
                ViewContentType.Pdf => PdfViewContent.Create(viewComponent, source),
                ViewContentType.Archive => ArchiveViewContent.Create(viewComponent, source),
                _ => new ViewContent(),
            };
            return viewContent;
        }
    }

}
