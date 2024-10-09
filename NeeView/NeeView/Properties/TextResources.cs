using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Properties
{
    /// <summary>
    /// NeeView.Properties.Resources に代わるテキストリソース
    /// </summary>
    internal class TextResources
    {
        private static bool _initialized;


        public static CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        public static FileLanguageResource LanguageResource { get; } = new();

        public static TextResourceManager Resource { get; } = new(LanguageResource);


        public static void Initialize(CultureInfo culture)
        {
            if (_initialized) throw new InvalidOperationException("Already initialized.");
            _initialized = true;

#if false
            // 開発用：各カルチャのテスト
            foreach (var c in LanguageResource.Cultures)
            {
                System.Diagnostics.Debug.WriteLine($"Culture: {c}");
                Resource.Load(c);
                Resource.Add(new AppFileSource(new Uri("/Languages/shared.restext", UriKind.Relative)));
                SearchOptionManual.OpenSearchOptionManual();
                System.Threading.Thread.Sleep(2000);
            }
#endif

            Culture = culture;
            Resource.Load(culture);
            Resource.Add(new AppFileSource(new Uri("/Languages/shared.restext", UriKind.Relative)));
        }

        /// <summary>
        /// 最低限の初期化。設定ファイル読み込み前のエラー等の正常初期化前の処理用。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitializeMinimum()
        {
            if (!_initialized)
            {
                Initialize(CultureInfo.CurrentCulture);
            }
        }

        public static string GetString(string name)
        {
            InitializeMinimum();
            return Resource.GetString(name) ?? "@" + name;
        }

        public static string? GetStringRaw(string name)
        {
            InitializeMinimum();
            return Resource.GetString(name);
        }

        public static string? GetStringRaw(string name, CultureInfo culture)
        {
            InitializeMinimum();
            return Resource.GetString(name, culture);
        }

        public static string GetCaseString(string name, string pattern)
        {
            InitializeMinimum();
            return Resource.GetCaseString(name, pattern) ?? "@" + name;
        }

        public static string GetFormatString(string name, object? arg0)
        {
            var pattern = arg0?.ToString() ?? "";
            return string.Format(GetCaseString(name, pattern), arg0);
        }
    }
}
