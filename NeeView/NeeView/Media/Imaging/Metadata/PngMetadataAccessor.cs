using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class PngMetadataAccessor : BitmapMetadataAccessor
    {
        private static readonly string _strElementPrefix = "/{str=";

        private readonly BitmapMetadata _meta;
        private readonly Dictionary<string, List<string>> _textMap = new();


        public PngMetadataAccessor(BitmapMetadata meta)
        {
            _meta = meta ?? throw new ArgumentNullException(nameof(meta));
            Debug.Assert(_meta.Format == "png");

            CollectItxtChunks();
            CollectTextChunks();
        }


        private void CollectItxtChunks()
        {
            foreach (var key in _meta.Where(e => e.EndsWith("iTXt", StringComparison.Ordinal)))
            {
                if (_meta.GetQuery(key) is BitmapMetadata itxt)
                {
                    if (itxt.GetQuery("/Keyword") is string keyword && itxt.GetQuery("/TextEntry") is string text)
                    {
                        AddToMap(keyword, text);
                    }
                }
            }
        }

        private void CollectTextChunks()
        {
            foreach (var key in _meta.Where(e => e.EndsWith("tEXt", StringComparison.Ordinal)))
            {
                if (_meta.GetQuery(key) is BitmapMetadata chunk)
                {

                    foreach (var title in chunk.Where(e => e.StartsWith(_strElementPrefix)))
                    {
                        var keywordLength = title.Length - _strElementPrefix.Length - 1;
                        if (keywordLength > 1)
                        {
                            var keyword = title.Substring(_strElementPrefix.Length, keywordLength);
                            if (chunk.GetQuery(title) is string text)
                            {
                                AddToMap(keyword, text);
                            }
                        }
                    }
                }
            }
        }

        private void AddToMap(string key, string text)
        {
            if (!_textMap.TryGetValue(key, out var strings))
            {
                strings = new List<string>();
                _textMap.Add(key, strings);
            }
            strings.Add(text);
        }

        public override string GetFormat()
        {
            return _meta.Format.ToUpper();
        }

        public override object? GetValue(BitmapMetadataKey key)
        {
            return key switch
            {
                BitmapMetadataKey.Title => GetPngText("Title"),
                BitmapMetadataKey.Subject => GetPngText("Description"),
                BitmapMetadataKey.Rating => null,// not supprted
                BitmapMetadataKey.Tags => null,// not supported
                BitmapMetadataKey.Comments => GetPngText("Comment"),
                BitmapMetadataKey.Author => GetPngTextCollection("Author"),
                BitmapMetadataKey.DateTaken => GetTime(),
                BitmapMetadataKey.ApplicatoinName => GetPngText("Software"),
                BitmapMetadataKey.Copyright => GetPngText("Copyright"),
                // NOTE: PNGメタデータテキストの Warning, Disclaimer, Source は対応項目がない
                _ => null,
            };
        }

        private object? GetTime()
        {
            if (_meta.GetQuery("/tIME") is BitmapMetadata time && false)
            {
                var timeMap = new Dictionary<string, int>();
                foreach (var item in time)
                {
                    timeMap[item] = Convert.ToInt32(time.GetQuery(item));
                }

                timeMap.TryGetValue("/Year", out int year);
                timeMap.TryGetValue("/Month", out int month);
                timeMap.TryGetValue("/Day", out int day);
                timeMap.TryGetValue("/Hour", out int hour);
                timeMap.TryGetValue("/Minute", out int minute);
                timeMap.TryGetValue("/Second", out int second);

                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).ToLocalTime();
            }

            var creationTime = GetPngText("Creation Time");
            if (creationTime != null)
            {
                if (ExifDateTime.TryParse(creationTime, out var exifDateTime))
                {
                    return exifDateTime;
                }
                else if (DateTime.TryParse(creationTime, out var dateTime))
                {
                    return dateTime;
                }
                return creationTime;
            }

            return null;
        }

        private string? GetPngText(string keyword)
        {
            if (_textMap.TryGetValue(keyword, out var strings))
            {
                return string.Join(System.Environment.NewLine, strings);
            }
            return null;
        }

        private ReadOnlyCollection<string>? GetPngTextCollection(string keyword)
        {
            if (_textMap.TryGetValue(keyword, out var strings))
            {
                return new ReadOnlyCollection<string>(strings);
            }
            return null;
        }
    }
}

