using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [Obsolete("no used")]
    public interface IPagemarkEntry : IHasName
    {
        string? Path { get; }
        string DispName { get; }
    }

    [Obsolete("no used")]
    public class Pagemark : BindableBase, IPagemarkEntry
    {
        private string _path;
        private string _entryName;
        private string? _dispName;


        public Pagemark(string path, string entryName)
        {
            _path = path;
            _entryName = entryName;
        }


        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(null);
                }
            }
        }

        public string EntryName
        {
            get { return _entryName; }
            set
            {
                if (SetProperty(ref _entryName, value))
                {
                    RaisePropertyChanged(nameof(DispName));
                }
            }
        }

        [NotNull]
        public string? DispName
        {
            get { return _dispName ?? LoosePath.GetFileName(EntryName); }
            set { SetProperty(ref _dispName, (string.IsNullOrWhiteSpace(value) || value == LoosePath.GetFileName(EntryName)) ? null : value); }
        }

        public string? DispNameRaw => _dispName;
        public string FullName => LoosePath.Combine(Path, EntryName);
        public string Name => EntryName;
        public string Note => LoosePath.GetFileName(Path);
        public string Detail => EntryName;


        public bool IsEqual(IPagemarkEntry entry)
        {
            return entry is Pagemark pagemark && this.Name == pagemark.Name && this.Path == pagemark.Path;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }
    }

}

