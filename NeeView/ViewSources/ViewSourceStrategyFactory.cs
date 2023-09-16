using NeeView.ComponentModel;
using System;

namespace NeeView
{
    public static class ViewSourceStrategyFactory
    {
        public static IViewSourceStrategy? Create(PageContent pageContent, DataSource data) // data は pageContent から求まるよね？
        {
            if (!data.IsLoaded) return null;

            // NOTE: エラーデータは事前に弾いておくこと
            if (data.IsFailed) return null;

            return data.Data switch
            {
                BitmapPageData => new BitmapViewSourceStrategy(pageContent),
                AnimatedPageData => new AnimatedViewSourceStrategy(pageContent),
                MediaPageData => new MediaViewSourceStrategy(pageContent),
                PdfPageData => new PdfViewSourceStrategy(pageContent),
                SvgPageData => new SvgViewSourceStrategy(pageContent),
                ArchivePageData => new ArchiveViewSourceStrategy(pageContent),
                FilePageData => new FileViewSourceStrategy(pageContent),
                _ => throw new NotSupportedException(),
            };
        }
    }

}
