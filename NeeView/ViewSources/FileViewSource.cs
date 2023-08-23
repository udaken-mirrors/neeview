using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FileViewSource : ViewSource
    {
        public FileViewSource(PageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
        }

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage);
            }
            else
            {
                var source = data.Data as FilePageSource ?? throw new InvalidOperationException(nameof(data));
                SetData(source, 0, null);
            }
            await Task.CompletedTask;
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
