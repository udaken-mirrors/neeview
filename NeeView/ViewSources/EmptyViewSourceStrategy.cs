using NeeView.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class EmptyViewSourceStrategy : IViewSourceStrategy
    {
        public EmptyViewSourceStrategy(PageContent pageContent)
        {
        }

        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            return await Task.FromResult(new DataSource(new EmptyViewData(), 0, null));
        }
    }


    public class EmptyViewData
    {
    }
}
