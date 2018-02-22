﻿using System;
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
        public MediaArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
        }

        public override string ToString()
        {
            return "MediaPlayer";
        }

        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            var fileInfo = new FileInfo(this.Path);

            var entry = new ArchiveEntry()
            {
                Archiver = this,
                Id = 0,
                Instance = null,
                RawEntryName = LoosePath.GetFileName(this.EntryName),
                Length = fileInfo.Length,
                LastWriteTime = fileInfo.LastWriteTime,
            };

            return new List<ArchiveEntry>() { entry };
        }

        public override bool IsSupported()
        {
            return MediaArchiverProfile.Current.IsEnabled;
        }

        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (this.IsDisposed) throw new ApplicationException("Archive already colosed.");
            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            // エントリのパスはダミーなのでアーカイブのパスのみ返す
            return Path;
        }

        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");
            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }

        #region IDisposable Support

        private bool _isDisposed;

        public override bool IsDisposed => _isDisposed;

        public override void Dispose()
        {
            _isDisposed = true;
            base.Dispose();
        }

        #endregion
    }
}