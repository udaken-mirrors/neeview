using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeView.ComponentModel;

namespace NeeView
{
    public interface IPageContent : IDataSource
    {
        public event EventHandler? ContentChanged;
        public event EventHandler? SizeChanged;

        public int Index { get; set; }
        public PageContentState State { get; set; }
        [Obsolete]
        public ArchiveEntry ArchiveEntry { get; } // TODO: ArchiveEntry と Entry は同じものなので統一する
        public ArchiveEntry Entry { get; }
        public Size Size { get; }
        public Color Color { get; }
        public PictureInfo? PictureInfo { get; }

        public Task LoadAsync(CancellationToken token);
        public void Unload();

        // TODO: NeeViewLegacy互換
        public FileProxy CreateTempFile(bool isKeepFileName);
    }

}
