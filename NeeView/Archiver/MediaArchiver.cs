using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            return Properties.TextResources.GetString("Archiver.Media");
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

        public override string GetEntryFullName(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            // MediaArchiver のエントリはダミーなのでアーカイブのパスをそのまま返す
            return entry.Archiver.SystemPath;
        }

        public override string GetEntryIdent(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            return entry.Archiver.Ident;
        }

        public override string GetSystemPath(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            return entry.Archiver.SystemPath;
        }

        /// <summary>
        /// エントリの実体パスを取得
        /// </summary>
        /// <param name="entry">エントリ</param>
        /// <returns>実体パス。アーカイブパス等実在しない場合は null</returns>
        public override string? GetEntityPath(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            return Path;
        }

        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            Debug.Assert(entry.Archiver == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            return await Task.FromResult(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archiver == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            await FileIO.CopyFileAsync(path, exportFileName, isOverwrite, token);
        }
    }
}
