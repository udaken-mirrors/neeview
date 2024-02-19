using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class PageLoadingViewModel : INotifyPropertyChanged
    {
        private PageLoading _model;

        public PageLoadingViewModel(PageLoading model)
        {
            _model = model;
            _model.SubscribePropertyChanged(nameof(_model.IsActive), (s, e) => RaisePropertyChanged(nameof(IsActive)));
            _model.SubscribePropertyChanged(nameof(_model.Message), (s, e) => RaisePropertyChanged(nameof(Message)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsActive
        {
            get => _model.IsActive;
            set => _model.IsActive = value;
        }

        public string Message
        {
            get => _model.Message;
            set => _model.Message = value;
        }
    }
}
