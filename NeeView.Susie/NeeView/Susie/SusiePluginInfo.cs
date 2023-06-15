using NeeLaboratory.Collections.Specialized;
using System;
using System.ComponentModel;

namespace NeeView.Susie
{
    public class SusiePluginInfo
    {
        public SusiePluginInfo(string name)
        {
            Name = name;
            DefaultExtension = new FileExtensionCollection();
        }


        public string Name { get; set; }

        public string? FileName { get; set; }

        public string? ApiVersion { get; set; }

        public string? PluginVersion { get; set; }

        public SusiePluginType PluginType { get; set; }

        public string? DetailText { get; set; }

        public bool HasConfigurationDlg { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsPreExtract { get; set; }

        public FileExtensionCollection DefaultExtension { get; set; }

        public FileExtensionCollection? UserExtension { get; set; }

        public FileExtensionCollection Extensions => UserExtension ?? DefaultExtension;

        public SusiePluginSetting ToSusiePluginSetting()
        {
            var setting = new SusiePluginSetting(Name);
            setting.IsEnabled = IsEnabled;
            setting.IsCacheEnabled = IsCacheEnabled;
            setting.IsPreExtract = IsPreExtract;
            setting.UserExtensions = UserExtension?.ToOneLine();
            return setting;
        }
    }
}
