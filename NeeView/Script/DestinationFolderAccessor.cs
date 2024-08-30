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
        public void CopyPage(PageAccessor page)
        {
            CopyPage([page]);
        }

        [WordNodeMember]
        public void CopyPage(PageAccessor[] pages)
        {
            var async = CopyAsync(pages, CancellationToken.None);
        }

        [WordNodeMember]
        public void Copy(string path)
        {
            Copy([path]);
        }

        [WordNodeMember]
        public void Copy(string[] paths)
        {
            var async = CopyAsync(paths, CancellationToken.None);
        }

        private async Task CopyAsync(PageAccessor[] pages, CancellationToken token)
        {
            // ページは実体化する
            var items = await CreateRealizedFilePathListAsync(pages.Select(e => e.Source), token);
            await _folder.CopyAsyncNoExceptions(items, token);
        }

        private async Task CopyAsync(string[] paths, CancellationToken token)
        {
            // ページは実体化する
            var map = paths.Select(e => (Key: e, Page: GetPage(e))).ToList();
            var items1 = map.Where(e => e.Page is null).Select(e => e.Key);
            var items2 = await CreateRealizedFilePathListAsync(map.Where(e => e.Page is not null).Select(e => e.Page).WhereNotNull(), token);
            var items = items1.Concat(items2).ToList();
            await _folder.CopyAsyncNoExceptions(items, token);
        }


        [WordNodeMember]
        public void MovePage(PageAccessor page)
        {
            MovePage([page]);
        }

        [WordNodeMember]
        public void MovePage(PageAccessor[] pages)
        {
            var paths = pages
                .Select(e => e.Source)
                .Where(e => e.ArchiveEntry.IsFileSystem)
                .Select(e => e.EntryFullName)
                .WhereNotNull()
                .ToArray();

            Move(paths);
        }

        [WordNodeMember]
        public void Move(string path)
        {
            Move([path]);
        }

        [WordNodeMember]
        public void Move(string[] paths)
        {
            var async = _folder.MoveAsyncNoExceptions(paths, CancellationToken.None);
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
