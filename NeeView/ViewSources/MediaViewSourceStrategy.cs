using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MediaViewSourceStrategy : IViewSourceStrategy
    {
        private readonly PageContent _pageContent;

        public MediaViewSourceStrategy(PageContent pageContent)
        {
            _pageContent = pageContent;
        }

        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not MediaPageData pageData) throw new InvalidOperationException(nameof(data.Data));

            var viewData = new MediaViewData(pageData.Path, null);
            await Task.CompletedTask;
            return new DataSource(viewData, 0, null);
        }
    }

}
