﻿using NeeLaboratory.Diagnostics;
using NeeView.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region Native

        internal static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);
        }

        #endregion

        private static App? _current;
        public static new App Current => _current ?? throw new InvalidOperationException("_current must not be null");


        // Fields

        private bool _isSplashScreenVisibled;
        private bool _isTerminated;
        private readonly int _tickBase = System.Environment.TickCount;
        private CommandLineOption? _option;
        private MultbootService? _multiBootService;


        // Properties

        // オプション設定
        public CommandLineOption Option => _option ?? throw new InvalidOperationException("_option must not be null");

        // システムロック
        public object Lock { get; } = new object();

        // 起動日時
        public DateTime StartTime { get; private set; }

        // 開発用：ストップウォッチ
        public Stopwatch Stopwatch { get; private set; } = new();

        // MainWindowはLoad完了している？
        public bool IsMainWindowLoaded { get; set; }


        /// <summary>
        /// アプリの起動時間(ms)取得
        /// </summary>
        public int TickCount => System.Environment.TickCount - _tickBase;



        /// <summary>
        /// Startup
        /// </summary>
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            _current = this;

            StartTime = DateTime.Now;
            Stopwatch.Start();

            // DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

#if TRACE_LOG
            var nowTime = DateTime.Now;
            var traceLogFilename = $"Trace{nowTime.ToString("yyMMdHHmmss")}.log";
            StreamWriter sw = new StreamWriter(traceLogFilename) { AutoFlush = true };
            TextWriterTraceListener twtl = new TextWriterTraceListener(TextWriter.Synchronized(sw), "MyListener");
            Trace.Listeners.Add(twtl);
            Trace.WriteLine($"Trace: Start ({nowTime})");
#endif

            // [開発用] ログ出力設定
            if (!string.IsNullOrEmpty(Environment.LogFile))
            {
                var twtl = new TextWriterTraceListener(Environment.LogFile, "TraceLog");
                Trace.Listeners.Add(twtl);
                Trace.AutoFlush = true;
                Trace.WriteLine(System.Environment.NewLine + new string('=', 80));
            }

            Trace.WriteLine($"App.Startup: PID={System.Environment.ProcessId}: {DateTime.Now}");

            // 未処理例外ハンドル
            InitializeUnhandledException();

            try
            {
                await InitializeAsync(e);
            }
            catch (OperationCanceledException ex)
            {
                Trace.WriteLine("InitializeCancelException: " + ex.Message);
                ShutdownWithoutSave();
                return;
            }

            Trace.WriteLine($"App.Initialized: {Stopwatch.ElapsedMilliseconds}ms");

            // インスタンス確定
            _ = ThemeManager.Current;

            // メインウィンドウ起動
            var mainWindow = new MainWindow();

            Interop.NVFpReset();
            mainWindow.Show();

            MessageDialog.IsShowInTaskBar = false;
        }


        /// <summary> 
        /// 初期化 
        /// </summary>
        private async Task InitializeAsync(StartupEventArgs e)
        {
            Interop.TryLoadNativeLibrary(Environment.LibrariesPath);

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // APPXデータフォルダ移動 (ver.38)
            Environment.CoorectLocalAppDataFolder();

            // コマンドライン引数処理
            _option = ParseArguments(e.Args);
            _option.Validate();

            // カレントディレクトリを実行ファイルの場所に変更。ファイルロック回避のため
            System.IO.Directory.SetCurrentDirectory(Environment.AssemblyFolder);

            // シフトキー起動は新しいウィンドウで
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Option.IsNewWindow = SwitchOption.on;
            }

            // 多重起動サービス起動
            _multiBootService = new MultbootService();

            // セカンドプロセス判定
            Environment.IsSecondProcess = _multiBootService.IsServerExists;

            Debug.WriteLine($"App.UserSettingLoading: {Stopwatch.ElapsedMilliseconds}ms");

            // 設定の読み込み 
            var setting = SaveData.Current.LoadUserSetting(true);
            var config = setting.Config ?? Config.Current;

            Debug.WriteLine($"App.UserSettingLoaded: {Stopwatch.ElapsedMilliseconds}ms");

            // 言語適用。初期化に影響するため優先して設定
            NeeView.Properties.Resources.Culture = CultureInfo.GetCultureInfo(config.System.Language);

            // スプラッシュスクリーン
            ShowSplashScreen(config);

            // 設定の適用
            UserSettingTools.Restore(setting, new ObjectMergeOption() { IsIgnoreEnabled = false });

            // ページマークのプレイリスト変換
            if (setting?.Format != null && setting.Format.CompareTo(new FormatVersion(Environment.SolutionName, 39, 0, 0)) < 0)
            {
                PagemarkToPlaylistConverter.PagemarkToPlaylist();
            }

            // 画像拡張子初期化
            if (Config.Current.Image.Standard.SupportFileTypes is null)
            {
                Config.Current.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupprtedFileTypes(Config.Current.Image.Standard.UseWicInformation);
            }

            Debug.WriteLine($"App.RestreSettings: {Stopwatch.ElapsedMilliseconds}ms");

            // バージョン表示
            if (this.Option.IsVersion)
            {
                var dialog = new VersionWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                dialog.ShowDialog();
                throw new OperationCanceledException("Disp Version Dialog");
            }

            // 多重起動制限になる場合、サーバーにパスを送って終了
            Debug.WriteLine($"CanStart: {CanStart(Config.Current)}: IsServerExists={_multiBootService.IsServerExists}, IsNewWindow={Option.IsNewWindow}, IsMultiBootEnabled={Config.Current.StartUp.IsMultiBootEnabled}");
            if (!CanStart(Config.Current))
            {
                await _multiBootService.RemoteLoadAsAsync(Option.Values);
                throw new OperationCanceledException("Already started.");
            }

            // テンポラリーの場所
            Config.Current.System.TemporaryDirectory = Temporary.Current.SetDirectory(Config.Current.System.TemporaryDirectory, true);

            // TextBox以外のコントロールのIMEを無効にする
            if (!Config.Current.System.IsInputMethodEnabled)
            {
                InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(false));
                InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(System.Windows.Controls.TextBox), new FrameworkPropertyMetadata(true));
            }
        }

        /// <summary>
        /// Show SplashScreen
        /// </summary>
        public void ShowSplashScreen(Config config)
        {
            if (config.StartUp.IsSplashScreenEnabled && CanStart(config))
            {
                if (_isSplashScreenVisibled) return;
                _isSplashScreenVisibled = true;
                var resourceName = "Resources/SplashScreen.png";
                var splashScreen = new SplashScreen(resourceName);
                splashScreen.Show(true);
                Debug.WriteLine($"App.ShowSplashScreen: {Stopwatch.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>
        /// 多重起動用実行可能判定
        /// </summary>
        private bool CanStart(Config config)
        {
            if (_multiBootService is null) return false;

            return !_multiBootService.IsServerExists || (Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : config.StartUp.IsMultiBootEnabled);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Terminate();
            Trace.WriteLine("App.Exit: done.");
        }

        /// <summary>
        /// PCシャットダウン時に呼ばれる
        /// </summary>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Trace.WriteLine($"App.SessionEnding: {e.ReasonSessionEnding}");
            Terminate();
        }


        /// <summary>
        /// 終了時処理。設定の保存等
        /// </summary>
        private void Terminate()
        {
            if (_isTerminated) return;
            _isTerminated = true;

            try
            {
                // 各種Dispose
                ApplicationDisposer.Current.Dispose();

                // 設定保存
                SaveDataSync.Current.SaveAll(false, false);

                // キャッシュDBのクリーンナップ
                ThumbnailCache.Current.Cleanup();

                // キャッシュDBを閉じる
                ThumbnailCache.Current.Dispose();

                // テンポラリファイル破棄
                Temporary.Current.RemoveTempFolder();

                Trace.WriteLine($"App.Terminate: {DateTime.Now}: Terminated.");
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "Application Terminate failed!!");
                Trace.Fail($"App.Terminate: {DateTime.Now}: {ex.ToStackString()}");
            }

            CallProcessTerminator();
        }

        /// <summary>
        /// 自プロセスがゾンビ化しても停止させる処理。
        /// 現象ではPdfDocumentによってゾンビプロセス化してしまうため。
        /// </summary>
        /// <seealso href="https://github.com/microsoft/CsWinRT/issues/1249"/>
        private void CallProcessTerminator()
        {
            var filename = Path.Combine(Environment.LibrariesPath, "Libraries\\NeeView.Terminator.exe");

            // 開発中は直接プロジェクトを参照する
            if (Environment.IsDevPackage)
            {
                filename = Path.GetFullPath(Path.Combine(Environment.AssemblyFolder, @"..\..\..\..\..\NeeView.Terminator\bin", Environment.PlatformName, Environment.ConfigType, @"net6.0\NeeView.Terminator.exe"));
            }

            // 10秒後にこのプロセスが残っていたら強制終了させる監視プロセスを発行
            var process = Process.GetCurrentProcess();
            var info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.FileName = filename;
            info.Arguments = $"{process.Id} \"{process.ProcessName}\" {process.StartTime.ToFileTime()} {10000}";
            Process.Start(info);
        }


        /// <summary>
        /// 保存しないでアプリをシャットダウン
        /// </summary>
        public void ShutdownWithoutSave()
        {
            SaveData.Current.DisableSave();
            Shutdown();
        }



    }
}
