using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NeeLaboratory.ComponentModel;
using System.Net.Http;

namespace NeeView
{
    /// <summary>
    /// バージョンチェッカー
    /// </summary>
    public class VersionChecker : BindableBase
    {
        private volatile bool _isChecking = false;
        private volatile bool _isChecked = false;
        private string? _message;


        public VersionChecker()
        {
            CurrentVersion = Environment.CheckVersion;
            LastVersion = new FormatVersion(Environment.SolutionName, 0, 0, 0);
        }

        public static string DownloadUri => Environment.DistributionUrl;

        public bool IsEnabled => Config.Current.System.IsNetworkEnabled && !Environment.IsAppxPackage && !Environment.IsCanaryPackage && !Environment.IsBetaPackage;

        public FormatVersion CurrentVersion { get; set; }
        public FormatVersion LastVersion { get; set; }

        public bool IsExistNewVersion { get; set; }

        public string? Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }


        public void CheckStart()
        {
            if (_isChecked || _isChecking) return;

            if (IsEnabled)
            {
                // チェック開始
                LastVersion = new FormatVersion(Environment.SolutionName, 0, 0, 0);
                Message = Properties.TextResources.GetString("VersionChecker.Message.Checking");
                Task.Run(() => CheckVersion(Environment.PackageTypeExtension));
            }
        }

        private async Task CheckVersion(string extension)
        {
            _isChecking = true;

            try
            {
                Debug.WriteLine($"CheckVersion: {CurrentVersion}, {DownloadUri}");
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(new Uri(DownloadUri));
                    response.EnsureSuccessStatusCode();
                    var text = await response.Content.ReadAsStringAsync();

#if DEBUG
                    ////extension = ".msi";
#endif

                    var regex = new Regex(@"NeeView(?<major>\d+)\.(?<minor>\d+)(?<arch>-[^\.]+)?" + Regex.Escape(extension));
                    var matches = regex.Matches(text);
                    if (matches.Count <= 0) throw new ApplicationException(Properties.TextResources.GetString("VersionChecker.Message.WrongFormat"));
                    foreach (Match match in matches)
                    {
                        var major = int.Parse(match.Groups["major"].Value);
                        var minor = int.Parse(match.Groups["minor"].Value);
                        var version = new FormatVersion(Environment.SolutionName, major, minor, 0);

                        Debug.WriteLine($"NeeView {major}.{minor} - {version:x8}: {match.Groups["arch"]?.Value}");
                        if (LastVersion.CompareTo(version) < 0)
                        {
                            LastVersion = version;
                        }
                    }

                    if (LastVersion == CurrentVersion)
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.Latest");
                    }
                    else if (LastVersion.CompareTo(CurrentVersion) < 0)
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.Unknown");
                    }
                    else
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.New");
                        IsExistNewVersion = true;
                        RaisePropertyChanged(nameof(IsExistNewVersion));
                    }

                    _isChecked = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Message = Properties.TextResources.GetString("VersionChecker.Message.Failed");
            }

            _isChecking = false;
        }
    }
}
