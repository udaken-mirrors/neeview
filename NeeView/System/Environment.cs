﻿using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace NeeView
{
    /// <summary>
    /// アプリの環境
    /// </summary>
    public static class Environment
    {
        private static string? _localApplicationDataPath;
        private static string? _userDataPath;
        private static string? _packageType;
        private static string? _revision;
        private static string? _dateVersion;
        private static bool? _isUseLocalApplicationDataFolder;
        private static string? _pdfRenderer;
        private static bool? _watermark;
        private static string? _logFile;
        private static Encoding? _encoding;
        private static string? _neeviewProfile;


        // TODO: static でなくてよい
        static Environment()
        {
            // エンコーディングプロバイダの登録
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ProcessId = System.Environment.ProcessId;

            AssemblyLocation = System.Environment.ProcessPath ?? throw new InvalidOperationException("Cannot get this AsemblyLocatation");
            AssemblyFolder = Path.GetDirectoryName(AssemblyLocation) ?? throw new InvalidOperationException("Cannot get this AsemblyFolder");

            var assembly = Assembly.GetExecutingAssembly();
            ValidateProductInfo(assembly);

            // Windows7では標準でTLS1.1,TLS1.2に対応していないので対応させる。バージョンチェック通信用。
            if (Windows7Tools.IsWindows7)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
        }


        public static event EventHandler? LocalApplicationDataRemoved;


        /// <summary>
        /// プロセスID
        /// </summary>
        public static int ProcessId { get; private set; }

        /// <summary>
        /// マルチ起動での2番目以降のプロセス
        /// </summary>
        public static bool IsSecondProcess { get; set; }

        /// <summary>
        /// アセンブリの場所
        /// </summary>
        public static string AssemblyLocation { get; private set; }

        /// <summary>
        /// アセンブリの場所
        /// </summary>
        public static string AssemblyFolder { get; private set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public static string CompanyName { get; private set; }

        /// <summary>
        /// ソリューション名
        /// </summary>
        public static string SolutionName => "NeeView";

        /// <summary>
        /// タイトル名
        /// </summary>
        public static string AssemblyTitle { get; private set; }

        /// <summary>
        /// プロダクト名
        /// </summary>
        public static string AssemblyProduct { get; private set; }

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public static Version AssemblyVersion { get; private set; }

        /// <summary>
        /// アプリ名
        /// </summary>
        public static string ApplicationName => AssemblyTitle;

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public static string ProductVersion { get; private set; }

        /// <summary>
        /// 表示用バージョン
        /// </summary>
        public static string DispVersion
        {
            get
            {
                if (IsCanaryPackage)
                {
                    return $"Canary {DateVersion} / Rev. {Revision}";
                }
                else if (IsBetaPackage)
                {
                    return ProductVersion + $".Beta {DateVersion} / Rev. {Revision}";
                }
                else
                {
                    return ProductVersion;
                }
            }
        }

        /// <summary>
        /// 表示用バージョン (ショート)
        /// </summary>
        public static string DispVersionShort
        {
            get
            {
                if (IsCanaryPackage)
                {
                    return $"Canary {DateVersion}";
                }
                else if (IsBetaPackage)
                {
                    return ProductVersion + $".Beta {DateVersion}";
                }
                else
                {
                    return ProductVersion;
                }
            }
        }

        /// <summary>
        /// プロダクトバージョン(int)
        /// </summary>
        public static int ProductVersionNumber { get; private set; }

        /// <summary>
        /// 環境変数 NEEVIEW_PROFILE 取得
        /// </summary>
        private static string NeeViewProfile
        {
            get
            {
                if (_neeviewProfile is null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    _neeviewProfile = GetEnvironmentValue("NEEVIEW_PROFILE").Trim();
                    if (!string.IsNullOrEmpty(_neeviewProfile))
                    {
                        if (!Path.IsPathRooted(_neeviewProfile))
                        {
                            // Error: 環境変数 NEEVIEW_PROFILE は絶対パスではありません
                            throw new IOException("NEEVIEW_PROFILE: Not an absolute path");
                        }
                        if (!Directory.Exists(_neeviewProfile))
                        {
                            // Error: 環境変数 NEEVIEW_PROFILE が示すディレクトリが存在しません: (path)
                            throw new DirectoryNotFoundException($"NEEVIEW_PROFILE: Directory not found: {_neeviewProfile}");
                        }
                    }
                }
                return _neeviewProfile;
            }
        }

        /// <summary>
        /// アプリケーションデータフォルダー
        /// </summary>
        public static string LocalApplicationDataPath
        {
            get
            {
                if (_localApplicationDataPath == null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    if (!string.IsNullOrEmpty(NeeViewProfile))
                    {
                        _localApplicationDataPath = NeeViewProfile;
                    }
                    // configファイルの設定で LocalApplicationData を使用するかを判定。インストール版用
                    else if (IsUseLocalApplicationDataFolder)
                    {
                        _localApplicationDataPath = GetLocalAppDataPath();
                        CreateFolder(_localApplicationDataPath);
                    }
                    // 既定ではアプリの場所の Profile フォルダーに作る
                    else
                    {
                        _localApplicationDataPath = Path.Combine(AssemblyFolder, "Profile");
                        CreateFolder(_localApplicationDataPath);
                    }
                    Debug.WriteLine($"LocalApplicationDataPath: {_localApplicationDataPath}");
                }

                return _localApplicationDataPath;
            }
        }


        /// <summary>
        /// ユーザーデータフォルダー
        /// ユーザーが直接編集する可能性のあるデータ(スクリプトとか)の場所を区別するため LocalApplicationDataPath とは別定義
        /// </summary>
        public static string UserDataPath
        {
            get
            {
                if (_userDataPath == null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    if (!string.IsNullOrEmpty(NeeViewProfile))
                    {
                        _userDataPath = NeeViewProfile;
                    }
                    // インストール版では MyDocument を使用
                    else if (IsUseLocalApplicationDataFolder)
                    {
                        _userDataPath = GetMyDocumentPath();
                        if (string.IsNullOrEmpty(_userDataPath))
                        {
                            _userDataPath = LocalApplicationDataPath;
                        }
                    }
                    // 既定では LocalApplicationDataPath
                    else
                    {
                        _userDataPath = LocalApplicationDataPath;
                    }
                    Debug.WriteLine($"UserDataPath: {_userDataPath}");
                }
                return _userDataPath;
            }
        }

        /// <summary>
        /// ライブラリーパス
        /// </summary>
        public static string LibrariesPath => AssemblyFolder;

        /// <summary>
        /// ライブラリーパス(Platform別)
        /// </summary>
        public static string LibrariesPlatformPath
        {
            get { return Path.Combine(LibrariesPath, PlatformName); }
        }

        /// <summary>
        /// x86/x64判定
        /// </summary>
        public static bool IsX64
        {
            get { return IntPtr.Size == 8; }
        }

        public static string PlatformName
        {
            get { return IsX64 ? "x64" : "x86"; }
        }

        // データ保存にアプリケーションデータフォルダーを使用するか
        public static bool IsUseLocalApplicationDataFolder
        {
            get
            {
                if (_isUseLocalApplicationDataFolder == null)
                {
                    _isUseLocalApplicationDataFolder = string.Compare(ConfigurationManager.AppSettings["UseLocalApplicationData"], "True", true) == 0;
                }
                return (bool)_isUseLocalApplicationDataFolder;
            }
        }

        // パッケージの種類(拡張子)
        public static string PackageType
        {
            get
            {
                if (_packageType == null)
                {
                    _packageType = ConfigurationManager.AppSettings["PackageType"] ?? ".zip";
                    ////if (_packageType != ".msi") _packageType = ".zip";
                }
                return _packageType;
            }
        }

        public static bool IsDevPackage => PackageType == ".dev";
        public static bool IsZipPackage => PackageType == ".zip";
        public static bool IsMsiPackage => PackageType == ".msi";
        public static bool IsAppxPackage => PackageType == ".appx";
        public static bool IsCanaryPackage => PackageType == ".canary";
        public static bool IsBetaPackage => PackageType == ".beta";

        public static bool IsZipLikePackage => IsZipPackage || IsCanaryPackage || IsBetaPackage || IsDevPackage;

#if DEBUG
        public static readonly string ConfigType = "Debug";
#else
        public static readonly string ConfigType = "Release";
#endif

        public static string Revision
        {
            get
            {
                if (_revision == null)
                {
                    _revision = ConfigurationManager.AppSettings["Revision"] ?? "??";
                }
                return _revision;
            }
        }

        public static string DateVersion
        {
            get
            {
                if (_dateVersion == null)
                {
                    _dateVersion = ConfigurationManager.AppSettings["DateVersion"] ?? "??";
                }
                return _dateVersion;
            }
        }

        /// <summary>
        /// システムのエンコーディング
        /// </summary>
        public static Encoding Encoding
        {
            get
            {
                if (_encoding is null)
                {
                    _encoding = Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
                }
                return _encoding;
            }
        }


        public static string PdfRenderer
        {
            get
            {
                if (_pdfRenderer is null)
                {
                    _pdfRenderer = ConfigurationManager.AppSettings["PdfRenderer"] ?? "Pdfium";
                }
                return _pdfRenderer;
            }
        }

        public static bool Watermark
        {
            get
            {
                if (_watermark is null)
                {
                    _watermark = string.Compare(ConfigurationManager.AppSettings["Watermark"], "True", true) == 0;
                }
                return (bool)_watermark;
            }
        }


        // [開発用] 出力用ログファイル名
        public static string LogFile
        {
            get
            {
                if (_logFile == null)
                {
                    var logFile = ConfigurationManager.AppSettings["LogFile"];
                    if (string.IsNullOrEmpty(logFile))
                    {
                        _logFile = "";
                    }
                    else if (Path.IsPathRooted(logFile))
                    {
                        _logFile = logFile;
                    }
                    else
                    {
                        _logFile = Path.Combine(LocalApplicationDataPath, logFile);
                    }
                }
                return _logFile;
            }
        }



        /// <summary>
        /// 環境変数取得
        /// </summary>
        /// <param name="variable">変数名</param>
        /// <returns>値。取得できないときは空文字列</returns>
        public static string GetEnvironmentValue(string variable)
        {
            try
            {
                return System.Environment.GetEnvironmentVariable(variable) ?? "";
            }
            catch (SecurityException)
            {
                return "";
            }
        }

        // ※ build は未使用
        public static int GenerateProductVersionNumber(int major, int minor, int build)
        {
            return major << 16 | minor << 8;
        }

        // プロダクトバージョン(int)からメジャーバージョンを取得
        public static int GetMajorVersionNumber(int versionNumber)
        {
            return (versionNumber >> 16) & 0xff;
        }

        // プロダクトバージョン(int)からマイナーバージョンを取得
        public static int GetMinorVersionNumber(int versionNumber)
        {
            return (versionNumber >> 8) & 0xff;
        }

        // PCメモリサイズ
        public static ulong GetTotalPhysicalMemory()
        {
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            return info.TotalPhysicalMemory;
        }

        /// <summary>
        /// アセンブリ情報収集
        /// </summary>
        /// <param name="asm"></param>
        [MemberNotNull(nameof(CompanyName), nameof(AssemblyTitle), nameof(AssemblyProduct), nameof(AssemblyVersion), nameof(ProductVersion), nameof(ProductVersionNumber))]
        private static void ValidateProductInfo(Assembly asm)
        {
            // 会社名
            AssemblyCompanyAttribute? companyAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            CompanyName = companyAttribute?.Company ?? throw new InvalidOperationException("Cannot get AssemblyCompany");

            // タイトル
            AssemblyTitleAttribute? titleAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            AssemblyTitle = titleAttribute?.Title ?? throw new InvalidOperationException("Cannot get AssemblyTitle");

            // プロダクト
            AssemblyProductAttribute? productAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            AssemblyProduct = productAttribute?.Product ?? throw new InvalidOperationException("Cannot get AssemblyProduct");

            // バージョンの取得
            AssemblyVersion = asm.GetName().Version ?? throw new InvalidOperationException("Cannot get AssemblyVersion");
            ProductVersion = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}";
            ProductVersionNumber = GenerateProductVersionNumber(AssemblyVersion.Major, AssemblyVersion.Minor, 0);
        }

        /// <summary>
        /// マイドキュメントのアプリ専用フォルダー
        /// </summary>
        /// <returns>マイドキュメントのパス。取得できないときは空文字</returns>
        public static string GetMyDocumentPath()
        {
            var myDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return string.IsNullOrEmpty(myDocuments) ? "" : System.IO.Path.Combine(myDocuments, CompanyName, SolutionName);

#if false
            if (string.IsNullOrEmpty(myDocuments))
            {
                myDocuments = LoosePath.TrimDirectoryEnd(System.Environment.GetEnvironmentVariable("SystemDrive"));
            }
            if (string.IsNullOrEmpty(myDocuments))
            {
                myDocuments = @"C:\";
            }

            return System.IO.Path.Combine(myDocuments, CompanyName, SolutionName);
#endif
        }

        /// <summary>
        /// データフォルダーを取得する
        /// </summary>
        /// <param name="name">フォルダー名</param>
        /// <returns>フルパス。取得できない場合はEmptyを返す</returns>
        public static string GetUserDataPath(string name)
        {
            if (string.IsNullOrEmpty(UserDataPath))
            {
                return "";
            }
            else
            {
                return Path.Combine(UserDataPath, name);
            }
        }

        /// <summary>
        /// フォルダー生成
        /// </summary>
        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// AppDataのアプリ用ローカルフォルダーのパスを取得
        /// </summary>
        public static string GetLocalAppDataPath()
        {
            if (IsAppxPackage)
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName + "-" + SolutionName);
            }
            else
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName, SolutionName);
            }
        }

        /// <summary>
        /// AppDataのカンパニーフォルダーのパスを取得
        /// </summary>
        /// <remarks>
        /// Appxではカンパニーフォルダーは存在しないのでnullになる
        /// </remarks>
        private static string? GetLocalAppDataCompanyPath()
        {
            if (IsAppxPackage)
            {
                return null;
            }
            else
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName);
            }
        }

        // 全ユーザデータ削除
        public static void RemoveApplicationData(Window? owner)
        {
            var dialog = new MessageDialog(Properties.TextResources.GetString("DeleteApplicationDataDialog.Message"), Properties.TextResources.GetString("DeleteApplicationDataDialog.Title"));
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(owner);

            if (result.Command == UICommands.Delete)
            {
                // キャッシュDBを閉じる
                ThumbnailCache.Current.Close();

                try
                {
                    RemoveApplicationDataCore();
                    new MessageDialog(Properties.TextResources.GetString("DeleteApplicationDataCompleteDialog.Message"), Properties.TextResources.GetString("DeleteApplicationDataCompleteDialog.Title")).ShowDialog(owner);
                    LocalApplicationDataRemoved?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Properties.TextResources.GetString("DeleteApplicationDataErrorDialog.Title")).ShowDialog(owner);
                }
            }
        }

        // 全ユーザデータ削除
        private static bool RemoveApplicationDataCore()
        {
            // LocalApplicationDataフォルダーを使用している場合のみ
            if (!IsUseLocalApplicationDataFolder)
            {
                throw new ApplicationException(Properties.TextResources.GetString("CannotDeleteDataException.Message"));
            }

            Debug.WriteLine("RemoveAllApplicationData ...");

            var productFolder = GetLocalAppDataPath();
            Directory.Delete(productFolder, true);
            System.Threading.Thread.Sleep(500);

            var companyFolder = GetLocalAppDataCompanyPath();
            if (companyFolder != null)
            {
                if (Directory.GetFileSystemEntries(companyFolder).Length == 0)
                {
                    Directory.Delete(companyFolder);
                }
            }

            Debug.WriteLine("RemoveAllApplicationData done.");
            return true;
        }


        /// <summary>
        /// APPXデータフォルダー移動 (ver.38)
        /// </summary>
        /// <remarks>
        /// これまでの NeeLaboratory\NeeView.a では NeeLaboratory フォルダーがインストール前に存在していると NeeView.a がアンインストールでも消えないため、
        /// ver.38からの専用のフォルダー NeeLaboratory-NeeView に移動させる。
        /// アプリ専用の仮想フォルダとして生成されるため、アンインストールで自動削除される
        /// </remarks>
        public static void CoorectLocalAppDataFolder()
        {
            // this function is spoort Appx only.
            if (!IsAppxPackage) return;
            if (!IsUseLocalApplicationDataFolder) return;

            try
            {
                string oldPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName, SolutionName) + ".a";
                string newPath = GetLocalAppDataPath();

                // if already exist new path, exit.
                if (Directory.Exists(newPath)) return;

                // if old path not exist, exit
                var directory = new DirectoryInfo(oldPath);
                if (!directory.Exists) return;

                // move ... OK?
                directory.MoveTo(newPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(CoorectLocalAppDataFolder) + " failed: " + ex.Message);
            }
        }
    }
}
