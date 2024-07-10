using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeView
{
    internal static class ResourceService
    {
        private static readonly Regex _regexKey = new(@"@[a-zA-Z0-9_\.#]+[a-zA-Z0-9]");
        private static readonly Regex _regexResKey = new Regex(@"@\[([^\]]+)\]");

        /// <summary>
        /// @で始まる文字列はリソースキーとしてその値を返す。
        /// そうでない場合はそのまま返す。
        /// </summary>
        public static string GetString(string? key)
        {
            if (string.IsNullOrWhiteSpace(key) || key[0] != '@')
            {
                return key ?? "";
            }
            else
            {
                var text = GetResourceString(key);
                if (text != null)
                {
                    return Replace(text);
                }
                else
                {
                    Debug.WriteLine($"Error: Not found resource key: {key[1..]}");
                    return key;
                }
            }
        }

        /// <summary>
        /// @で始まる文字列をリソースキーとして文字列を入れ替える。
        /// </summary>
        public static string Replace(string s)
        {
            // limit is 5 depth
            return Replace(s, 5);
        }

        public static string Replace(string s, int recursionLimit)
        {
            if (recursionLimit <= 0) return s;

            return ReplaceEmbeddedText(_regexKey.Replace(s, ReplaceMatchEvaluator));
         
            string ReplaceMatchEvaluator(Match m)
            {
                var s = GetResourceString(m.Value);
                return s is not null ? Replace(s, recursionLimit - 1) : m.Value;
            }
        }

        /// <summary>
        /// @[...] という文字列をテキストリソースのパスとして文字列を入れ替える
        /// </summary>
        public static string ReplaceEmbeddedText(string s)
        {
            return _regexResKey.Replace(s, FileNameToTextMatchEvaluator);
        }

        private static string FileNameToTextMatchEvaluator(Match match)
        {
            var fileName = match.Groups[1].Value;
            var fileSource = new AppFileSource(new Uri(fileName, UriKind.Relative));
            using var stream = fileSource.Open();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <returns>存在しない場合は null</returns>
        public static string? GetResourceString(string key)
        {
            if (key is null || key[0] != '@') return null;
            //return Properties.TextResources.GetString("ResourceManager").GetString(key[1..], Properties.TextResources.GetString("Culture"));
            return Properties.TextResources.GetStringRaw(key[1..]);
        }

        /// <summary>
        /// リソースキーからリソース文字列取得
        /// </summary>
        /// <param name="key">@で始まるリソースキー</param>
        /// <param name="isRecursive">結果に含まれるキーを変換する</param>
        /// <returns>存在しない場合は null</returns>
        public static string? GetResourceString(string key, bool isRecursive)
        {
            var text = GetResourceString(key);

            if (text != null && isRecursive)
            {
                return Replace(text);
            }
            else
            {
                return text;
            }
        }


        /// <summary>
        /// 連結単語文字列を生成
        /// </summary>
        public static string Join(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens.Select(e => string.Format(Properties.TextResources.GetStringRaw("TokenFormat") ?? "", e)));
        }
    }
}
