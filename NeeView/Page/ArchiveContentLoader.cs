using NeeView.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace NeeView
{
    public interface IHasInitializeEntry
    {
        Task InitializeEntryAsync(CancellationToken token);
    }

    public class ArchiveContentLoader : BitmapContentLoader, IHasInitializeEntry
    {
        private static readonly AsyncLock _lock = new();

        private readonly ArchiveContent _content;

        public ArchiveContentLoader(ArchiveContent content) : base(content)
        {
            _content = content;
        }

        public async Task InitializeEntryAsync(CancellationToken token)
        {
            if (_content.Entry.IsEmpty)
            {
                var query = new QueryPath(_content.SourcePath);
                query = query.ToEntityPath();
                try
                {
                    _content.SetEntry(await ArchiveEntryUtility.CreateAsync(query.SimplePath, token));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                    _content.SetEntry(ArchiveEntry.Create(query.SimplePath));
                    _content.Thumbnail.Initialize(null);
                }
            }
        }

        /// <summary>
        /// コンテンツロード
        /// </summary>
        public override async Task LoadContentAsync(CancellationToken token)
        {
            await InitializeEntryAsync(token);

            if (_content.IsLoaded) return;

            try
            {
                await LoadThumbnailAsync(token);
            }
            finally
            {
                RaiseLoaded();
                _content.UpdateDevStatus();
            }
        }


        /// <summary>
        /// サムネイルロード
        /// </summary>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            await InitializeEntryAsync(token);

            bool isLoadCache = true;
            if (!Config.Current.Thumbnail.IsVideoThumbnailEnabled && _content.Entry.IsMedia())
            {
                isLoadCache = false;
            }

            if (isLoadCache)
            {
                await _content.Thumbnail.InitializeAsync(_content.Entry, null, token);
                if (_content.Thumbnail.IsValid) return;
            }

            if (!_content.Entry.IsValid && !_content.Entry.IsArchivePath)
            {
                _content.Thumbnail.Initialize(null);
                return;
            }

            try
            {
                var picture = await LoadPictureAsync(token);
                token.ThrowIfCancellationRequested();
                if (picture == null)
                {
                    _content.Thumbnail.Initialize(null);
                }
                else if (picture.Type == ThumbnailType.Unique)
                {
                    _content.Thumbnail.Initialize(picture.RawData);
                }
                else
                {
                    _content.Thumbnail.Initialize(picture.Type);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // 例外無効
                Debug.WriteLine($"LoadThumbnail: {e.Message}");
                _content.Thumbnail.Initialize(null);
            }
        }


        /// <summary>
        /// エントリに対応するサムネイル画像生成
        /// </summary>
        private async Task<ThumbnailPicture?> LoadPictureAsync(CancellationToken token)
        {
            if (_content.Entry.Archiver != null && _content.Entry.Archiver is MediaArchiver)
            {
                return await LoadMediaPictureAsync(_content.Entry, token);
            }
            if (_content.Entry.IsArchivePath)
            {
                var entry = await ArchiveEntryUtility.CreateAsync(_content.Entry.SystemPath, token);
                if (entry.IsBook())
                {
                    return await LoadArchivePictureAsync(entry, token);
                }
                else
                {
                    return new ThumbnailPicture(CreateThumbnail(entry, token));
                }
            }
            else
            {
                return await LoadArchivePictureAsync(_content.Entry, token);
            }
        }

        /// <summary>
        /// アーカイブサムネイル読込
        /// 名前順で先頭のページ
        /// </summary>
        private static async Task<ThumbnailPicture?> LoadArchivePictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            // ブックサムネイル検索範囲
            const int searchRange = 2;

            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiverManager.Current.GetSupportedType(entry.SystemPath) == ArchiverType.MediaArchiver)
                {
                    return await LoadMediaPictureAsync(entry, token);
                }

                var select = await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, searchRange, token);
                if (select != null)
                {
                    return new ThumbnailPicture(CreateThumbnail(select, token));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ThumbnailPicture(CreateThumbnail(entry, token));
            }
        }

        private static byte[] CreateThumbnail(ArchiveEntry entry, CancellationToken token)
        {
            var source = PictureSourceFactory.Create(entry, null, PictureSourceCreateOptions.IgnoreCompress, token);
            return MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(ThumbnailProfile.Current, token));
        }


        private static async ValueTask<ThumbnailPicture> LoadMediaPictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (Config.Current.Thumbnail.IsVideoThumbnailEnabled && entry.IsFileSystem)
            {
                var thumbnail = await CreateMediaThumbnailAsync(entry, token);
                if (thumbnail != null)
                {
                    return new ThumbnailPicture(thumbnail);
                }
            }
            return new ThumbnailPicture(ThumbnailType.Media);
        }

        private static async Task<byte[]?> CreateMediaThumbnailAsync(ArchiveEntry entry, CancellationToken token)
        {
            var storage = await StorageFile.GetFileFromPathAsync(entry.SystemPath).AsTask(token);
            if (storage is null)
            {
                return null;
            }
            // NOTE: 複数同時に取得しようとすると失敗することがあるので排他制御にする
            using (await _lock.LockAsync(token))
            {
                using var thumbnail = await storage.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView, (uint)Config.Current.Thumbnail.ImageWidth, ThumbnailOptions.ResizeThumbnail).AsTask(token);
                if (thumbnail is null)
                {
                    return null;
                }
                return CreateThumbnailImage(thumbnail.AsStream(), Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality);
            }
        }

        private static byte[] CreateThumbnailImage(Stream bitmapStream, BitmapImageFormat format, int quality)
        {
            using (var outStream = new MemoryStream())
            {
                var bitmap = BitmapFrame.Create(bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var encoder = CreateFormat(format, quality);
                encoder.Frames.Add(bitmap);
                encoder.Save(outStream);
                return outStream.ToArray();
            }
        }

        // from PdfPictureSource.cs
        private static BitmapEncoder CreateFormat(BitmapImageFormat format, int quality)
        {
            return format switch
            {
                BitmapImageFormat.Png => new PngBitmapEncoder(),
                _ => new JpegBitmapEncoder() { QualityLevel = quality },
            };
        }

        /// <summary>
        /// 画像、もしくはサムネイルタイプを指定するもの
        /// </summary>
        class ThumbnailPicture
        {
            public ThumbnailType Type { get; set; }
            public byte[]? RawData { get; set; }

            public ThumbnailPicture(ThumbnailType type)
            {
                Type = type;
            }

            public ThumbnailPicture(byte[] rawData)
            {
                Type = ThumbnailType.Unique;
                RawData = rawData;
            }
        }
    }
}
