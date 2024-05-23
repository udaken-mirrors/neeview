using Esprima;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public record class DestinationFolderAccessor
    {
        private readonly DestinationFolder _folder;

        public DestinationFolderAccessor(DestinationFolder folder)
        {
            _folder = folder;
        }

        internal DestinationFolder Source => _folder;

        [WordNodeMember]
        public string Name
        {
            get { return _folder.Name; }
            set { AppDispatcher.Invoke(() => _folder.Name = value); }
        }

        [WordNodeMember]
        public string Path
        {
            get { return _folder.Path; }
            set { AppDispatcher.Invoke(() => _folder.Path = value); }
        }


        [WordNodeMember]
        public void Copy(PageAccessor page)
        {
            Copy([page]);
        }

        [WordNodeMember]
        public void Copy(PageAccessor[] pages)
        {
           _folder.Copy(CreateRealizedFilePathList(pages.Select(e => e.Source)));
        }

        [WordNodeMember]
        public void Copy(string path)
        {
            Copy([path]);
        }

        [WordNodeMember]
        public void Copy(string[] paths)
        {
            var pages = paths.Select(e => GetPage(e)).ToList();
            if (pages.All(e => e is not null))
            {
                _folder.Copy(CreateRealizedFilePathList(pages.WhereNotNull()));
            }
            else
            {
                _folder.Copy(paths);
            }
        }


        [WordNodeMember]
        public void Move(PageAccessor page)
        {
            Move([page]);
        }

        [WordNodeMember]
        public void Move(PageAccessor[] pages)
        {
            Move(pages.Select(e => e.Source));
        }

        [WordNodeMember]
        public void Move(string path)
        {
            Move([path]);
        }

        [WordNodeMember]
        public void Move(string[] paths)
        {
            var pages = paths.Select(e => GetPage(e)).ToList();
            if (pages.All(e => e is not null))
            {
                Move(pages.WhereNotNull());
            }
            else
            {
                _folder.Move(paths);
            }
        }

        private void Move(IEnumerable<Page> pages)
        {
            // 移動可能なのはファイルシステムエントリのみ
            var movePages = pages.Where(e => e.ArchiveEntry.IsFileSystem).ToList();
            var paths = movePages.Select(e => e.GetFilePlace()).WhereNotNull().ToList();

            _folder.Move(paths);

            BookOperation.Current.BookControl.ValidateRemoveFile(movePages);
        }


        private static Page? GetPage(string path)
        {
            return BookHub.Current.GetCurrentBook()?.Pages.GetPageWithEntryFullName(path);
        }

        private static async Task<List<string>> CreateRealizedFilePathListAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            return await PageUtility.CreateRealizedFilePathListAsync(pages, token);
        }

        private static List<string> CreateRealizedFilePathList(IEnumerable<Page> pages)
        {
            return Task.Run(async () => await PageUtility.CreateRealizedFilePathListAsync(pages, CancellationToken.None).ConfigureAwait(false)).Result;
        }

    }
}
