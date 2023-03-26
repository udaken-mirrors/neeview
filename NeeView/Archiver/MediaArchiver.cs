using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class MediaArchiver : Archiver
    {
        public MediaArchiver(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override string ToString()
        {
            return "MediaPlayer";
        }

        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            var fileInfo = new FileInfo(this.Path);

            var entry = new ArchiveEntry(this)
            {
                IsValid = true,
                Id = 0,
                Instance = null,
                RawEntryName = LoosePath.GetFileName(this.EntryName),
                Length = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
            };

            await Task.CompletedTask;
            return new List<ArchiveEntry>() { entry };
        }

        public override bool IsSupported()
        {
            return Config.Current.Archive.Media.IsEnabled;
        }

        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            // エントリのパスはダミーなのでアーカイブのパスのみ返す
            return Path;
        }

        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }
    }
}
