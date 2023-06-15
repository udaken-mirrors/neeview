using NeeLaboratory.Collections.Specialized;
using System;

namespace NeeView.Susie
{
    public class SusiePluginSetting
    {
        public SusiePluginSetting(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsPreExtract { get; set; }

        public string? UserExtensions { get; set; }


        public SusiePluginInfo ToSusiePluginInfo()
        {
            var info = new SusiePluginInfo(Name);
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.UserExtension = UserExtensions is not null ? new FileExtensionCollection(UserExtensions) : null;
            return info;
        }
    }
}
