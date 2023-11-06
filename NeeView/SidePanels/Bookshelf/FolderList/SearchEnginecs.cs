using NeeLaboratory.IO.Search.FileSearch;
using NeeLaboratory.IO.Search.FileNode;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.IO.Search;

namespace NeeView
{
    /// <summary>
    /// 検索エンジン
    /// </summary>
    public class SearchEngine : IDisposable
    {
        public static NodeSearcher DefaultSearcher { get; } = new NodeSearcher();

        /// <summary>
        /// インデックスフィルタ用無効パス
        /// </summary>
        private static readonly List<string> _ignores = new()
        {
            // Windows フォルダを除外
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows),
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows) + ".old",
        };

        /// <summary>
        /// 検索エンジン
        /// </summary>
        private readonly NeeLaboratory.IO.Search.FileSearch.SearchEngine _engine;



        public SearchEngine(string path, bool includeSubdirectories)
        {
            Path = path;
            IncludeSubdirectories = includeSubdirectories;

            ////Debug.WriteLine($"SearchEngine: {path}");
            _engine = new NeeLaboratory.IO.Search.FileSearch.SearchEngine();
            _engine.Context.NodeFilter = NodeFilter;
            _engine.SetSearchAreas(new List<NodeArea> { new NodeArea(path, includeSubdirectories) });
        }



        public bool IsBusy => _engine.State != SearchCommandEngineState.Idle;

        public string Path { get; private set; }

        public bool IncludeSubdirectories { get; private set; }



        /// <summary>
        /// インデックスフィルタ
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool NodeFilter(FileSystemInfo info)
        {
            // 属性フィルター
            if ((info.Attributes & (FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Temporary)) != 0)
            {
                return false;
            }

            if ((info.Attributes & FileAttributes.Hidden) != 0 && !Config.Current.System.IsHiddenFileVisibled)
            {
                return false;
            }

            // ディレクトリ無効フィルター
            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                var infoFullName = info.FullName;
                var infoLen = infoFullName.Length;

                foreach (var ignore in _ignores)
                {
                    var ignoreLen = ignore.Length;

                    if (ignoreLen == infoLen || (ignoreLen < infoLen && infoFullName[ignoreLen] == '\\'))
                    {
                        if (infoFullName.StartsWith(ignore, true, null))
                        {
                            return false;
                        }
                    }
                }
            }

            // 対応アーカイブ判定。ショートカットもアーカイブの可能性があるため有効とする
            else
            {
                if (!ArchiverManager.Current.IsSupported(info.Name, false) && !FileShortcut.IsShortcut(info.Name))
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<SearchKey> Analyze(string keyword)
        {
            return _engine.Analyze(keyword);
        }

        public async Task<SearchResultWatcher> SearchAsync(string keyword, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // 検索
            var result = await _engine.SearchAsync(keyword.Trim(), token);

            // 監視開始
            var watcher = new SearchResultWatcher(_engine, result);
            return watcher;
        }

        public void CancelSearch()
        {
            _engine.CancelSearch();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

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
                    _engine.CancelSearch();
                    _engine.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
