using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [Obsolete("no used")]
    public class PagemarkFolder : BindableBase, IPagemarkEntry
    {
        private static readonly IThumbnail _thumbnail = new FolderThumbnail();
        private string? _path;

        public string? Path
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

        public string? Name => _path;
        public string DispName => LoosePath.GetFileName(_path);
        public string? Note => null;
        public string? Detail => _path;

        public IThumbnail Thumbnail => _thumbnail;


        public static string GetValidateName(string name)
        {
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public bool IsEqual(IPagemarkEntry entry)
        {
            return entry is PagemarkFolder folder && this.Path == folder.Path;
        }
    }

    [Obsolete("no used")]
    public class PagemarkEmpty : IPagemarkEntry
    {
        public string Path => "";

        public string DispName => "";

        public string Name => "";
    }
}
