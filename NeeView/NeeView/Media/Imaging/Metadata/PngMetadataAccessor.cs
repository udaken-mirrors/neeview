using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class PngMetadataAccessor : BitmapMetadataAccessor
    {
        private static readonly string _strElementPrefix = "/{str=";

        private static readonly Dictionary<BitmapMetadataKey, string> _tagMap = new()
        {
            [BitmapMetadataKey.Title] = "Title",
            [BitmapMetadataKey.Subject] = "Description",
            [BitmapMetadataKey.Rating] = "",
            [BitmapMetadataKey.Tags] = "",
            [BitmapMetadataKey.Comments] = "Comment",
            [BitmapMetadataKey.Author] = "Author",
            [BitmapMetadataKey.DateTaken] = "Creation Time",
            [BitmapMetadataKey.ApplicationName] = "Software",
            [BitmapMetadataKey.Copyright] = "Copyright",
        };

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

                    foreach (var title in chunk.Where(e => e.StartsWith(_strElementPrefix, StringComparison.Ordinal)))
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
            return _meta.Format.ToUpperInvariant();
        }

        public override object? GetValue(BitmapMetadataKey key)
        {
            return key switch
            {
                BitmapMetadataKey.Title => GetPngText(_tagMap[BitmapMetadataKey.Title]),
                BitmapMetadataKey.Subject => GetPngText(_tagMap[BitmapMetadataKey.Subject]),
                BitmapMetadataKey.Rating => null,// not supprted
                BitmapMetadataKey.Tags => null,// not supported
                BitmapMetadataKey.Comments => GetPngText(_tagMap[BitmapMetadataKey.Comments]),
                BitmapMetadataKey.Author => GetPngTextCollection(_tagMap[BitmapMetadataKey.Author]),
                BitmapMetadataKey.DateTaken => GetTime(),
                BitmapMetadataKey.ApplicationName => GetPngText(_tagMap[BitmapMetadataKey.ApplicationName]),
                BitmapMetadataKey.Copyright => GetPngText(_tagMap[BitmapMetadataKey.Copyright]),
                // NOTE: PNGメタデータテキストの Warning, Disclaimer, Source は対応項目がない
                _ => null,
            };
        }

        public override Dictionary<string, object?> GetExtraValues()
        {
            var extras = _textMap.Keys.Cast<string>()
                .Except(_tagMap.Values)
                .ToDictionary(e => e, e => (object?)GetPngText(e));

            return extras;
        }

        private object? GetTime()
        {
            // tIMEチャンクを優先
            if (_meta.GetQuery("/tIME") is BitmapMetadata time && false)
            {
                var timeMap = new Dictionary<string, int>();
                foreach (var item in time)
                {
                    timeMap[item] = Convert.ToInt32(time.GetQuery(item), CultureInfo.InvariantCulture);
                }

                timeMap.TryGetValue("/Year", out int year);
                timeMap.TryGetValue("/Month", out int month);
                timeMap.TryGetValue("/Day", out int day);
                timeMap.TryGetValue("/Hour", out int hour);
                timeMap.TryGetValue("/Minute", out int minute);
                timeMap.TryGetValue("/Second", out int second);

                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).ToLocalTime();
            }

            var creationTime = GetPngText(_tagMap[BitmapMetadataKey.DateTaken]);
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

