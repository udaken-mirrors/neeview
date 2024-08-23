using NeeLaboratory.Collections.Specialized;
using System;
using System.ComponentModel;

namespace NeeView.Susie
{
    public class SusiePluginInfo : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private string _name;
        private string? _fileName;
        private string? _apiVersion;
        private string? _pluginVersion;
        private SusiePluginType _pluginType;
        private string? _detailText;
        private bool _hasConfigurationDlg;
        private bool _isEnabled;
        private bool _isCacheEnabled;
        private bool _isPreExtract;
        private FileExtensionCollection _defaultExtension;
        private FileExtensionCollection? _userExtension;


        public SusiePluginInfo(string name)
        {
            _name = name;
            _defaultExtension = new FileExtensionCollection();
        }


        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string? FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        public string? ApiVersion
        {
            get { return _apiVersion; }
            set { SetProperty(ref _apiVersion, value); }
        }

        public string? PluginVersion
        {
            get { return _pluginVersion; }
            set { SetProperty(ref _pluginVersion, value); }
        }

        public SusiePluginType PluginType
        {
            get { return _pluginType; }
            set { SetProperty(ref _pluginType, value); }
        }

        public string? DetailText
        {
            get { return _detailText; }
            set { SetProperty(ref _detailText, value); }
        }

        public bool HasConfigurationDlg
        {
            get { return _hasConfigurationDlg; }
            set { SetProperty(ref _hasConfigurationDlg, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set { SetProperty(ref _isCacheEnabled, value); }
        }

        public bool IsPreExtract
        {
            get { return _isPreExtract; }
            set { SetProperty(ref _isPreExtract, value); }
        }

        public FileExtensionCollection DefaultExtension
        {
            get { return _defaultExtension; }
            set
            {
                if (SetProperty(ref _defaultExtension, value))
                {
                    RaisePropertyChanged(nameof(Extensions));
                }
            }
        }

        public FileExtensionCollection? UserExtension
        {
            get { return _userExtension; }
            set
            {
                var extension = (value is null || value.Equals(_defaultExtension)) ? null : value;
                if (SetProperty(ref _userExtension, extension))
                {
                    RaisePropertyChanged(nameof(Extensions));
                }
            }
        }

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

        public void Set(SusiePluginInfo info)
        {
            Name = info.Name;
            FileName = info.FileName;
            ApiVersion = info.ApiVersion;
            PluginVersion = info.PluginVersion;
            PluginType = info.PluginType;
            DetailText = info.DetailText;
            HasConfigurationDlg = info.HasConfigurationDlg;
            IsEnabled = info.IsEnabled;
            IsCacheEnabled = info.IsCacheEnabled;
            IsPreExtract = info.IsPreExtract;
            DefaultExtension = new FileExtensionCollection(info.DefaultExtension);
            UserExtension = info.UserExtension is not null ? new FileExtensionCollection(info.UserExtension) : null;
        }
    }
}
