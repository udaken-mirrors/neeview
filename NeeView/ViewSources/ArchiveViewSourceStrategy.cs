using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ArchiveViewSourceStrategy : IViewSourceStrategy
    {
        private readonly PageContent _pageContent;

        public ArchiveViewSourceStrategy(PageContent pageContent)
        {
            _pageContent = pageContent;
        }

        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not ArchivePageData pageData) throw new InvalidOperationException(nameof(data.Data));

            var bitmapSource = await AppDispatcher.InvokeAsync(() =>
            {
                var bitmapSourceCollection = pageData.ArchiveEntry.IsDirectory
                    ? FileIconCollection.Current.CreateDefaultFolderIcon()
                    : FileIconCollection.Current.CreateFileIcon(pageData.ArchiveEntry.EntryFullName, IO.FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                return bitmapSourceCollection.GetBitmapSource(48.0);
            });

            return new DataSource(new ArchiveViewData(pageData.ArchiveEntry, new ThumbnailBitmap(pageData.Thumbnail), bitmapSource), 0, null);
        }
    }

}
