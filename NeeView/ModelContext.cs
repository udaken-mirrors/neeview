﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public static partial class Win32Api
    {
        // 参考：自然順ソート？
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    //
    public static class ModelContext
    {
        public static JobEngine JobEngine { get; set; }

        public static SusieContext SusieContext { get; set; }
        public static Susie.Susie Susie => ModelContext.SusieContext.Susie;

        public static BookHistory BookHistory { get; set; }

        public static ArchiverManager ArchiverManager { get; set; }
        public static BitmapLoaderManager BitmapLoaderManager { get; set; }

        public static void Initialize()
        {
            JobEngine = new JobEngine();
            JobEngine.Start();

            BookHistory = new BookHistory();

            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();

            SusieContext = new SusieContext();
            SusieContext.Initialize(null);
        }

        public static void Terminate()
        {
            JobEngine.Dispose();
        }
    }


    public class SusieContext
    {
        public Susie.Susie Susie { get; private set; }

        public bool _IsEnableSusie;
        public bool IsEnableSusie
        {
            get { return _IsEnableSusie; }
            set
            {
                if (_IsEnableSusie == value) return;
                _IsEnableSusie = value;
                SusieBitmapLoader.IsEnable = _IsEnableSusie;
            }
        }

        public string _SusiePluginPath;
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set
            {
                if (_SusiePluginPath != value)
                {
                    _SusiePluginPath = value;
                    Initialize(_SusiePluginPath);
                }
            }
        }

        public bool _IsFirstOrderSusieImage;
        public bool IsFirstOrderSusieImage
        {
            get { return _IsFirstOrderSusieImage; }
            set
            {
                if (_IsFirstOrderSusieImage != value)
                {
                    _IsFirstOrderSusieImage = value;
                    Page.LoaderOrder = _IsFirstOrderSusieImage ? BitmapLoaderType.Susie : BitmapLoaderType.Default;
                }
            }
        }

        public bool _IsFirstOrderSusieArchive;
        public bool IsFirstOrderSusieArchive
        {
            get { return _IsFirstOrderSusieArchive; }
            set
            {
                if (_IsFirstOrderSusieArchive != value)
                {
                    _IsFirstOrderSusieArchive = value;
                    ModelContext.ArchiverManager.OrderType = _IsFirstOrderSusieArchive ? ArchiverType.SusieArchiver : ArchiverType.DefaultArchiver;
                }
            }
        }


        // Susie s初期化
        public void Initialize(string pluginPath)
        {
            // 新規
            Susie = new Susie.Susie();
            if (pluginPath != null) Susie.SearchPath.Add(pluginPath);
            Susie.Initialize();

            // Susie対応拡張子更新
            ModelContext.ArchiverManager.UpdateSusieSupprtedFileTypes(Susie);
            ModelContext.BitmapLoaderManager.UpdateSusieSupprtedFileTypes(Susie);
        }


        [DataContract]
        public class SusieSetting
        {
            [DataMember]
            public bool IsEnableSusie { get; set; }

            [DataMember]
            public string SusiePluginPath { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieImage { get; set; }

            [DataMember]
            public bool IsFirstOrderSusieArchive { get; set; }


            private void Constructor()
            {
                SusiePluginPath = global::Susie.Susie.GetSusiePluginInstallPath();
            }

            public SusieSetting()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        public SusieSetting CreateMemento()
        {
            var memento = new SusieSetting();
            memento.IsEnableSusie = IsEnableSusie;
            memento.SusiePluginPath = SusiePluginPath;
            memento.IsFirstOrderSusieImage = IsFirstOrderSusieImage;
            memento.IsFirstOrderSusieArchive = IsFirstOrderSusieArchive;
            return memento;
        }

        public void Restore(SusieSetting memento)
        {
            IsEnableSusie = memento.IsEnableSusie;
            SusiePluginPath = memento.SusiePluginPath;
            IsFirstOrderSusieImage = memento.IsFirstOrderSusieImage;
            IsFirstOrderSusieArchive = memento.IsFirstOrderSusieArchive;
        }
    }
}

