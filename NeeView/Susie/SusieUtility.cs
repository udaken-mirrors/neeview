using Microsoft.Win32;

namespace NeeView.Susie
{
    public static class SusieUtility
    {
        private static string? _susiePluginInstallPath;

        // レジストリに登録されているSusiePluginパスの取得
        public static string GetSusiePluginInstallPath()
        {
            if (_susiePluginInstallPath is not null) return _susiePluginInstallPath;

            try
            {
                RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"Software\Takechin\Susie\Plug-in", false);
                _susiePluginInstallPath = (string?)regKey?.GetValue("Path") ?? "";
            }
            catch
            {
                _susiePluginInstallPath = "";
            }

            return _susiePluginInstallPath;
        }
    }
}
