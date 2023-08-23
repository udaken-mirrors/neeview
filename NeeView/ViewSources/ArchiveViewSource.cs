using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace NeeView
{
    public class ArchiveViewSource : ViewSource
    {
        private PageContent _pageContent;

        public ArchiveViewSource(PageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent;
        }

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage);
            }
            else
            {
                var thumbnail = data.Data as Thumbnail ?? throw new InvalidOperationException(nameof(data));
                SetData(new ArchiveViewData(_pageContent.Entry, thumbnail), 0, null);
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

    public class ArchiveViewData
    {
        public ArchiveViewData(ArchiveEntry entry, Thumbnail thumbnail)
        {
            Entry = entry;
            Thumbnail = thumbnail;
        }

        public ArchiveEntry Entry { get; }
        public Thumbnail Thumbnail { get; }
    }
}
