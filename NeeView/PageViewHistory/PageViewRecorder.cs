using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NeeView
{
    [DataContract]
    public class PageViewRecorder : BindableBase, IDisposable
    {
        private static PageViewRecorder? _current;
        public static PageViewRecorder Current => _current ?? throw new InvalidOperationException();


        public static void Initialize()
        {
            if (_current is not null) return;
            _current = new PageViewRecorder();
        }



        private FileStream? _file;
        private StringBuilder? _writeBuffer;
        private DateTime _viewedPagesDateTime;
        private List<Page>? _viewedPages;
        private DateTime _viewedBookDateTime;
        private string? _viewedBookAddress;
        private string? _viewedBookName;
        private object _lock = new object();
        private bool _disposedValue;
        private DisposableCollection _disposables = new DisposableCollection();


        private PageViewRecorder()
        {
            _disposables.Add(Config.Current.PageViewRecorder.SubscribePropertyChanged(OnPropertyChanged));

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);

            UpdateState();
        }


        private void WritePageViewedRecord(DateTime now)
        {
            lock (_lock)
            {
                if (_file is null) return;
                if (_writeBuffer is null) return;
                if (_viewedPages is null) return;

                _writeBuffer.Clear();
                foreach (var page in _viewedPages)
                {
                    _writeBuffer.Append(_viewedPagesDateTime.ToString("O"));
                    _writeBuffer.Append('\t');
                    switch (page.PageType)
                    {
                        case PageType.Folder:
                            _writeBuffer.Append("Folder");
                            break;
                        case PageType.File:
                            _writeBuffer.Append("File");
                            break;
                        default:
                            _writeBuffer.Append("Unknown");
                            break;
                    }
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append((now - _viewedPagesDateTime).TotalSeconds.ToString("#0.0000000"));
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append(page.BookPrefix);
                    _writeBuffer.Append('\t');
                    _writeBuffer.Append(page.EntryName);
                    _writeBuffer.AppendLine();
                }

                WriteString(_writeBuffer.ToString());
            }
        }

        private void WriteBookViewedRecord(DateTime now)
        {
            lock (_lock)
            {
                if (_file is null) return;
                if (_writeBuffer is null) return;
                if (_viewedBookAddress is null) return;

                _writeBuffer.Clear();
                _writeBuffer.Append(_viewedBookDateTime.ToString("O"));
                _writeBuffer.Append('\t');
                _writeBuffer.Append("Book");
                _writeBuffer.Append('\t');
                _writeBuffer.Append((now - _viewedBookDateTime).TotalSeconds.ToString("#0.0000000"));
                _writeBuffer.Append('\t');
                _writeBuffer.Append(_viewedBookAddress);
                _writeBuffer.Append('\t');
                _writeBuffer.Append(_viewedBookName);
                _writeBuffer.AppendLine();

                WriteString(_writeBuffer.ToString());
            }
        }

        private void WriteString(string text)
        {
            if (_file is null) return;

            try
            {
                App.Current.SemaphoreWait();
                var bytes = Encoding.UTF8.GetBytes(text);
                _file.Seek(0L, SeekOrigin.End);
                _file.Write(bytes, 0, bytes.Length);
                _file.Flush();
            }
            catch (IOException err)
            {
                Debug.WriteLine("[Error] {0}", err.Message);
                ToastService.Current.Show(new Toast(Resources.PageViewRecordWriteError_Message, "", ToastIcon.Error));
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        private void OpenFile(string path)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                try
                {
                    _file = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _writeBuffer = new StringBuilder(1024);
                }
                catch (IOException err)
                {
                    Debug.WriteLine("[Error] {0}", err.Message);
                    ToastService.Current.Show(new Toast(Resources.PageViewRecordWriteError_Message, "", ToastIcon.Error));
                }
                _viewedBookAddress = null;
                _viewedPages = new List<Page>();
                BookHub.Current.BookChanged += OnViewBookChanged;
                BookHub.Current.ViewContentsChanged += OnViewContentsChanged;
            }
        }

        private void CloseFile()
        {
            if (_disposedValue) return;

            if (_file != null)
            {
                var now = DateTime.Now;
                WritePageViewedRecord(now);
                WriteBookViewedRecord(now);
            }

            lock (_lock)
            {
                BookHub.Current.ViewContentsChanged -= OnViewContentsChanged;
                BookHub.Current.BookChanged -= OnViewBookChanged;
                try
                {
                    _file?.Close();
                }
                catch (Exception err)
                {
                    Debug.WriteLine("[Skip] {0}", err.Message);
                }
                finally
                {
                    _writeBuffer = null;
                    _file = null;
                }
            }
        }


        private void OnViewBookChanged(object? sender, BookChangedEventArgs e)
        {
            if (_disposedValue) return;

            var now = DateTime.Now;
            var book = BookHub.Current.Book;

            WriteBookViewedRecord(now);

            _viewedBookDateTime = now;

            if (book == null)
            {
                _viewedBookAddress = null;
                return;
            }

            _viewedBookAddress = book.Address;
            if (book.NotFoundStartPage != null && book.Pages.Count > 0)
            {
                _viewedBookName = string.Format(Resources.Notice_CannotOpen, LoosePath.GetFileName(book.NotFoundStartPage));
            }
            else
            {
                _viewedBookName = LoosePath.GetFileName(book.Address);
            }
        }

        private void OnViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            var now = DateTime.Now;
            var viewedPages = e?.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();

            WritePageViewedRecord(now);
            _viewedPagesDateTime = now;
            _viewedPages = viewedPages;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;

            UpdateState();
        }

        private void UpdateState()
        {
            CloseFile();

            if (!Config.Current.PageViewRecorder.IsSavePageViewRecord)
            {
                return;
            }

            var filePath = Config.Current.PageViewRecorder.PageViewRecordFilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            OpenFile(filePath);
        }

        #region IDisposable Support

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    CloseFile();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Memento

        /// <summary>
        /// PageViewRecorder Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase, IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(false)]
            public bool IsRecordPageView { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string? PageViewRecordPath { get; set; }

            public void RestoreConfig(Config config)
            {
                config.PageViewRecorder.IsSavePageViewRecord = IsRecordPageView;
                config.PageViewRecorder.PageViewRecordFilePath = PageViewRecordPath;
            }
        }

        #endregion
    }
}
