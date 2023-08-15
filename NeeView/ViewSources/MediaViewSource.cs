using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace NeeView
{
    public class MediaViewSource : ViewSource
    {
        private MediaPageContent _pageContent;

        public MediaViewSource(IPageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent as MediaPageContent ?? throw new ArgumentException("need MediaPageContent", nameof(pageContent));
        }

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage);
            }
            else
            {
                SetData(data.Data, 0, null);
            }
            await Task.CompletedTask;
        }

        public override void Unload()
        {
            if (Data is not null)
            {
                SetData(null, 0, null);
            }
        }
    }
}
