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

            await Task.CompletedTask;
            return new DataSource(new ArchiveViewData(_pageContent.ArchiveEntry, pageData.Thumbnail), 0, null);
        }
    }

}
