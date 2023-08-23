using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // ファイルページ用コンテンツのアイコン
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alert,
    }

    /// <summary>
    /// ファイルページ用コンテンツ
    /// FilePageControl のパラメータとして使用される
    /// </summary>
    public class FilePageContent : PageContent
    {
        private FilePageSource _source;

        public FilePageContent(ArchiveEntry archiveEntry, FilePageIcon icon, string? message, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
            _source = new FilePageSource(archiveEntry, icon, message);
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            await Task.CompletedTask;
            return new PageSource(_source, null, new PictureInfo(DefaultSize));
        }
    }

    public class FilePageSource 
    {
        public FilePageSource(ArchiveEntry entry, FilePageIcon icon, string? message)
        {
            Entry = entry;
            Icon = icon;
            Message = message;
        }

        public ArchiveEntry Entry { get; }
        public FilePageIcon Icon { get; }
        public string? Message { get; }
    }

}
