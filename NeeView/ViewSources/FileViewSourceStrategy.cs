using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FileViewSourceStrategy : IViewSourceStrategy
    {
        private readonly PageContent _pageContent;

        public FileViewSourceStrategy(PageContent pageContent)
        {
            _pageContent = pageContent;
        }

        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not FilePageData pageData) throw new InvalidOperationException(nameof(data.Data));

            await Task.CompletedTask;
            return new DataSource(new FileViewData(pageData), 0, null);
        }
    }

}
