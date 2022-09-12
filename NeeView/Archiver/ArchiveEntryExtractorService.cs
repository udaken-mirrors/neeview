using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryExtractor管理
    /// キャンセルされたがまだ処理が残っているインスタンスの再利用
    /// </summary>
    public class ArchiveEntryExtractorService
    {
        static ArchiveEntryExtractorService() => Current = new ArchiveEntryExtractorService();
        public static ArchiveEntryExtractorService Current { get; }


        // lock object
        private readonly object _lock = new();

        /// <summary>
        /// キャンセルされたが処理中のインスタンス群
        /// </summary>
        private readonly Dictionary<string, ArchiveEntryExtractor> _collection = new();



        /// <summary>
        /// 指定したキーの削除
        /// </summary>
        /// <returns>削除されたオブジェクトを返す。ない場合はnull</returns>
        private ArchiveEntryExtractor? Remove(string key)
        {
            lock (_lock)
            {
                if (_collection.Remove(key, out var extractor))
                {
                    return extractor;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// キーの追加
        /// </summary>
        private void Add(string key, ArchiveEntryExtractor extractor)
        {
            lock (_lock)
            {
                _collection.Add(key, extractor);
            }
        }


        /// <summary>
        /// 展開
        /// テンポラリファイルはキャッシュを活用する
        /// </summary>
        public async Task<TempFile> ExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            var tempFile = TempFileCache.Current.Get(entry.Ident);
            if (tempFile != null) return tempFile;

            tempFile = await ExtractRawAsync(entry, token);
            TempFileCache.Current.Add(entry.Ident, tempFile);
            return tempFile;
        }

        /// <summary>
        /// 展開
        /// ファイルはテンポラリに生成される
        /// </summary>
        /// <returns>展開後されたファイル名</returns>
        public async Task<TempFile> ExtractRawAsync(ArchiveEntry entry, CancellationToken token)
        {
            //Debug.WriteLine($"EXT: {entry.Ident}");

            ArchiveEntryExtractor? extractor = null;
            try
            {
                extractor = this.Remove(entry.Ident);
                if (extractor == null)
                {
                    //Debug.WriteLine($"EXT:{entry.Ident} Create");
                    extractor = new ArchiveEntryExtractor(entry);
                    extractor.Expired += Extractor_Expired;
                }
                var tempFile = await extractor.WaitAsync(token);
                extractor.Dispose();
                return tempFile;
            }
            catch (OperationCanceledException)
            {
                //Debug.WriteLine($"EXT: {entry.Ident} Add to Reserver");
                if (extractor != null)
                {
                    this.Add(entry.Ident, extractor);
                }
                throw;
            }
        }

        // 期限切れ処理
        private void Extractor_Expired(object? sender, ArchiveEntry entry)
        {
            if (entry is null) return;

            //Debug.WriteLine($"EXT: Remove {entry.Ident}");
            var extractor = Remove(entry.Ident);
            extractor?.Dispose();
        }
    }
}
