using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class FileAssociationAccessor : INotifyPropertyChanged, IFileAssociation
    {
        private readonly FileAssociation _source;
        private bool _isEnabled;

        public FileAssociationAccessor(FileAssociation source)
        {
            _source = source;
            _isEnabled = _source.IsEnabled;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsDirty
        {
            get { return _isEnabled != _source.IsEnabled; }
        }

        public FileAssociationCategory Category => _source.Category;

        public string Extension => _source.Extension;

        public string? Description => _source.Description;


        public bool Flush()
        {
            if (!IsDirty) return false;

            _source.IsEnabled = _isEnabled;
            return true;
        }
    }

}