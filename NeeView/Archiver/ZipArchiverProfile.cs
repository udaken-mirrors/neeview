﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ZipArchiverProfile : BindableBase
    {
        static ZipArchiverProfile() => Current = new ZipArchiverProfile();
        public static ZipArchiverProfile Current { get; }


        private bool _isEnabled = true;


        private ZipArchiverProfile()
        {
        }

        [PropertyMember("@ParamZipArchiverIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamZipArchiverSupportFileTypes", Tips = "@ParamZipArchiverSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".zip");

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(".zip")]
            public string SupportFileTypes { get; set; }

            public void RestoreConfig()
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.SupportFileTypes = this.SupportFileTypes.OneLine;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.SupportFileTypes.OneLine = memento.SupportFileTypes;
        }

        #endregion

    }
}
