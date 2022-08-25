using SevenZip;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// SevenZipSharpのインスタンスアクセサ。
    /// 一時的にファイルロックを解除できるようにしている。
    /// </summary>
    public class SevenZipAccessor : IDisposable
    {
        private static bool _isLibraryInitialized;

        public static void InitializeLibrary()
        {
            if (_isLibraryInitialized) return;

            string dllPath = Environment.IsX64 ? Config.Current.Archive.SevenZip.X64DllPath : Config.Current.Archive.SevenZip.X86DllPath;
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                dllPath = System.IO.Path.Combine(Environment.LibrariesPlatformPath, "7z.dll");
            }

            SevenZipExtractor.SetLibraryPath(dllPath);

            FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            Debug.WriteLine("7z.dll: ver" + dllVersionInfo?.FileVersion);

            _isLibraryInitialized = true;
        }


        private string _fileName;
        private SevenZipExtractor? _extractor;
        private object _lock = new object();


        public SevenZipAccessor(string fileName)
        {
            InitializeLibrary();
            _fileName = fileName;
        }


        public string Format
        {
            get
            {
                lock (_lock)
                {
                    return GetExtractor().Format.ToString();
                }
            }
        }

        public bool IsSolid
        {
            get
            {
                lock (_lock)
                {
                    return GetExtractor().IsSolid;
                }
            }
        }


        public ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData
        {
            get
            {
                lock (_lock)
                {
                    // TODO: 重い処理。キャンセルできるようにしたい
                    return GetExtractor().ArchiveFileData;
                }
            }
        }


        /// <summary>
        /// エクストラクタの取得
        /// </summary>
        private SevenZipExtractor GetExtractor()
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_extractor is null)
                {
                    _extractor = new SevenZipExtractor(_fileName);
                }

                return _extractor;
            }
        }

        /// <summary>
        /// エクストラクタを開放し、ファイルをアンロックする
        /// </summary>
        public void Unlock()
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                _extractor?.Dispose();
                _extractor = null;
            }
        }


        public void ExtractFile(int index, Stream extractStream)
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                GetExtractor().ExtractFile(index, extractStream);
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    // NOTE: ファイルロックを解除する
                    _extractor?.Dispose();
                    _extractor = null;

                    _disposedValue = true;
                }
            }
        }

        ~SevenZipAccessor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
