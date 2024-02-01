using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// テキストリソース生成
    /// </summary>
    public class TextResourceFactory
    {
        private readonly LanguageResource _languageResource;

        public TextResourceFactory(LanguageResource languageResource)
        {
            _languageResource = languageResource;
        }

        public TextResourceSet Load(CultureInfo culture)
        {
            var culture0 = _languageResource.DefaultCulture;
            var culture1 = culture;

            var res0 = LoadResText(culture0);
            var res = res0;

            if (!culture1.Equals(culture0))
            {
                var res1 = LoadResText(culture1);
                res = res1.Concat(res0).GroupBy(e => e.Key, (key, keyValues) => keyValues.First());
            }

            return new TextResourceSet(culture1, res.ToDictionary(e => e.Key, e => e.Value));
        }

        private IEnumerable<KeyValuePair<string, string>> LoadResText(CultureInfo culture)
        {
            return LoadResText(_languageResource.CreateResTextFileName(culture));
        }

        private static IEnumerable<KeyValuePair<string, string>> LoadResText(string path)
        {
            if (!File.Exists(path))
            {
                return new List<KeyValuePair<string, string>>();
            }

            return File.ReadLines(path)
                .Select(e => Parse(e))
                .OfType<KeyValuePair<string, string>>();

            static KeyValuePair<string, string>? Parse(string s)
            {
                s = s.Trim();
                if (string.IsNullOrEmpty(s) || s[0] == ';' || s[0] == '#')
                {
                    return null;
                }
                var tokens = s.Split('=', 2);
                if (tokens.Length != 2)
                {
                    Debug.WriteLine($"ResText: FormatException");
                    return null;
                }
                return new(tokens[0].Trim(), tokens[1].Trim());
            }
        }


    }
}
