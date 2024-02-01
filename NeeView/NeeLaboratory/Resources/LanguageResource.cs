using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// 言語リソース
    /// </summary>
    public class LanguageResource
    {
        private const string _ext = ".restext";

        private string _path;
        private readonly CultureInfo _defaultCulture;
        private List<CultureInfo>? _cultures;


        public LanguageResource() : this("")
        {
        }

        public LanguageResource(string path)
        {
            _path = path;
            _defaultCulture = CultureInfo.GetCultureInfo("en");
        }


        /// <summary>
        /// 既定のカルチャ
        /// </summary>
        public CultureInfo DefaultCulture => _defaultCulture;

        /// <summary>
        /// 選択可能なカルチャリスト
        /// </summary>
        public IReadOnlyList<CultureInfo> Cultures => _cultures ?? new();



        /// <summary>
        /// 言語ファイルフォルダ
        /// </summary>
        public void SetFolder(string value)
        {
            if (_path != value)
            {
                _path = value;
                _cultures = null;
            }
        }

        /// <summary>
        /// リソースが存在するカルチャリストを作成する
        /// </summary>
        public void Load()
        {
            if (_cultures is not null) return;
            if (string.IsNullOrEmpty(_path)) throw new InvalidOperationException();

            _cultures = Directory.GetFiles(_path, "*" + _ext)
                .Select(e => GetCultureInfoFromFileName(e))
                .WhereNotNull()
                .OrderBy(e => e.NativeName)
                .ToList();
        }

        /// <summary>
        /// リソースファイル名前からカルチャを得る
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static CultureInfo? GetCultureInfoFromFileName(string fileName)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                return CultureInfo.GetCultureInfo(name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// リソースが存在するカルチャに変換する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public CultureInfo ValidateCultureInfo(CultureInfo culture)
        {
            if (culture.Equals(CultureInfo.InvariantCulture)) return _defaultCulture;
            Load();
            var result = _cultures?.FirstOrDefault(e => culture.Equals(e));
            return result ?? ValidateCultureInfo(culture.Parent);
        }

        /// <summary>
        /// カルチャからリソースファイル名を作成する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public string CreateResTextFileName(CultureInfo culture)
        {
            return Path.Combine(_path, culture.Name + _ext);
        }

    }
}
