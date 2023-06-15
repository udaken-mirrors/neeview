using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using NeeView.Susie;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NeeView.Susie.Client;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using NeeView.Properties;
using NeeLaboratory.Collections.Specialized;

namespace NeeView
{
    public class SusiePluginManager : BindableBase
    {
        static SusiePluginManager() => Current = new SusiePluginManager();
        public static SusiePluginManager Current { get; }

        private bool _isInitialized;
        private readonly SusiePluginRemoteClient _remote;
        private SusiePluginClient? _client;
        private List<SusiePluginInfo> _unauthorizedPlugins;
        private ObservableCollection<SusiePluginInfo> _INPlugins;
        private ObservableCollection<SusiePluginInfo> _AMPlugins;


        private SusiePluginManager()
        {
            _unauthorizedPlugins = new List<SusiePluginInfo>();
            _INPlugins = new ObservableCollection<SusiePluginInfo>();
            _AMPlugins = new ObservableCollection<SusiePluginInfo>();

            _remote = new SusiePluginRemoteClient();

        }


        public List<SusiePluginInfo> UnauthorizedPlugins
        {
            get { return _unauthorizedPlugins; }
            private set { _unauthorizedPlugins = value ?? new List<SusiePluginInfo>(); }
        }

        public ObservableCollection<SusiePluginInfo> INPlugins
        {
            get { return _INPlugins; }
            private set { SetProperty(ref _INPlugins, value); }
        }

        public ObservableCollection<SusiePluginInfo> AMPlugins
        {
            get { return _AMPlugins; }
            private set { SetProperty(ref _AMPlugins, value); }
        }

        public IEnumerable<SusiePluginInfo> Plugins
        {
            get
            {
                foreach (var plugin in UnauthorizedPlugins) yield return plugin;
                foreach (var plugin in INPlugins) yield return plugin;
                foreach (var plugin in AMPlugins) yield return plugin;
            }
        }

        /// <summary>
        /// 対応画像ファイル拡張子
        /// </summary>
        public FileTypeCollection ImageExtensions = new();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions = new();



        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsEnabled), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanging(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                CloseSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            UpdateSusiePluginCollection();
        }


        private void Plugins_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                FlushSusiePluginOrder();
            }
        }


        // PluginCollectionのOpen/Close
        private void UpdateSusiePluginCollection()
        {
            if (!_isInitialized) throw new InvalidOperationException();

            if (Config.Current.Susie.IsEnabled && Directory.Exists(Config.Current.Susie.SusiePluginPath))
            {
                OpenSusiePluginCollection();
            }
            else
            {
                CloseSusiePluginCollection();
            }
        }


        private void OpenSusiePluginCollection()
        {
            CloseSusiePluginCollection();

            _client = new SusiePluginClient(_remote);
            _client.SetRecoveryAction(LoadSusiePlugins);

            try
            {
                LoadSusiePlugins();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ToastService.Current.Show(new Toast(Resources.SusieConnectError_Message, null, ToastIcon.Error));
            }
        }

        private void LoadSusiePlugins()
        {
            if (_client is null) throw new InvalidOperationException();

            var settings = Plugins.Select(e => e.ToSusiePluginSetting()).ToList();
            _client.Initialize(System.IO.Path.GetFullPath(Config.Current.Susie.SusiePluginPath), settings);

            var plugins = _client.GetPlugin(null);
            UnauthorizedPlugins = new List<SusiePluginInfo>();
            INPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
            INPlugins.CollectionChanged += Plugins_CollectionChanged;
            AMPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));
            AMPlugins.CollectionChanged += Plugins_CollectionChanged;

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }

        private void CloseSusiePluginCollection()
        {
            if (_client == null) return;

            _client = null;

            UnauthorizedPlugins = Plugins.ToList();
            INPlugins = new ObservableCollection<SusiePluginInfo>();
            AMPlugins = new ObservableCollection<SusiePluginInfo>();

            UpdateImageExtensions();
            UpdateArchiveExtensions();
        }


        // Susie画像プラグインのサポート拡張子を更新
        public void UpdateImageExtensions()
        {
            var extensions = INPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ImageExtensions.Restore(extensions);

            Debug.WriteLine("SusieIN Support: " + string.Join(" ", this.ImageExtensions));
        }

        // Susies書庫プラグインのサポート拡張子を更新
        public void UpdateArchiveExtensions()
        {
            var extensions = AMPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ArchiveExtensions.Restore(extensions);

            Debug.WriteLine("SusieAM Support: " + string.Join(" ", this.ArchiveExtensions));
        }

        public void FlushSusiePluginSetting(string name)
        {
            if (_client is null) throw new InvalidOperationException();

            var settings = Plugins
                .Where(e => e.Name == name)
                .Select(e => e.ToSusiePluginSetting())
                .ToList();

            _client.SetPlugin(settings);
        }

        public void UpdateSusiePlugin(string name)
        {
            if (_client is null) throw new InvalidOperationException();

            var plugins = _client.GetPlugin(new List<string>() { name });
            if (plugins != null && plugins.Count == 1)
            {
                var collection = plugins[0].PluginType == SusiePluginType.Image ? INPlugins : AMPlugins;
                var plugin = collection.FirstOrDefault(e => e.Name == name);
                if (plugin is not null)
                {
                    var index = collection.IndexOf(plugin);
                    if (index >= 0)
                    {
                        collection[index] = plugins[0];
                    }
                }
            }
        }

        public void FlushSusiePluginOrder()
        {
            _client?.SetPluginOrder(Plugins.Select(e => e.Name).ToList());
        }

        public SusieImagePluginAccessor GetImagePluginAccessor()
        {
            if (_client is null) throw new InvalidOperationException();

            return new SusieImagePluginAccessor(_client, null);
        }

        public SusieImagePluginAccessor? GetImagePluginAccessor(string fileName, byte[] buff, bool isCheckExtension)
        {
            if (_client is null) throw new InvalidOperationException();

            var plugin = _client.GetImagePlugin(fileName, buff, isCheckExtension);
            if (plugin is null) return null;

            return new SusieImagePluginAccessor(_client, plugin);
        }

        public SusieArchivePluginAccessor? GetArchivePluginAccessor(string fileName, byte[]? buff, bool isCheckExtension)
        {
            if (_client is null) throw new InvalidOperationException();

            var plugin = _client.GetArchivePlugin(fileName, buff, isCheckExtension);
            if (plugin is null) return null;

            return new SusieArchivePluginAccessor(_client, plugin);
        }

        public void ShowPluginConfigulationDialog(string pluginName, Window owner)
        {
            if (_client is null) throw new InvalidOperationException();

            var handle = new WindowInteropHelper(owner).Handle;
            _client.ShowConfigulationDlg(pluginName, handle.ToInt32());
        }

        #region Memento

        public SusiePluginCollection CreateSusiePluginCollection()
        {
            var collection = new SusiePluginCollection();
            foreach(var item in this.Plugins)
            {
                collection.Add(item.Name, SusiePluginMemento.FromSusiePluginInfo(item));
            }
            return collection;
        }

        public void RestoreSusiePluginCollection(SusiePluginCollection? memento)
        {
            if (memento == null) return;
            this.UnauthorizedPlugins = memento.Select(e => e.Value.ToSusiePluginInfo(e.Key)).ToList();

            if (_isInitialized)
            {
                UpdateSusiePluginCollection();
            }
        }

        #endregion Memento
    }

    public class SusiePluginCollection : Dictionary<string, SusiePluginMemento>
    {
    }

    [Memento]
    public class SusiePluginMemento
    {
        public bool IsEnabled { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsPreExtract { get; set; }

        public string? UserExtensions { get; set; }


        public static SusiePluginMemento FromSusiePluginInfo(SusiePluginInfo info)
        {
            var setting = new SusiePluginMemento();
            setting.IsEnabled = info.IsEnabled;
            setting.IsCacheEnabled = info.IsCacheEnabled;
            setting.UserExtensions = info.UserExtension?.ToOneLine();
            return setting;
        }

        public static SusiePluginMemento FromSusiePluginSetting(Susie.SusiePluginSetting setting)
        {
            var memento = new SusiePluginMemento();
            memento.IsEnabled = setting.IsEnabled;
            memento.IsCacheEnabled = setting.IsCacheEnabled;
            memento.UserExtensions = setting.UserExtensions;
            return memento;
        }

        public SusiePluginInfo ToSusiePluginInfo(string name)
        {
            var info = new SusiePluginInfo(name);
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.UserExtension = UserExtensions != null ? new FileExtensionCollection(UserExtensions) : null;
            return info;
        }
    }

}
