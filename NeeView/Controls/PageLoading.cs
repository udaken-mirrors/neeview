using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class PageLoading : INotifyPropertyChanged
    {
        private Locker _locker;
        private bool _isActive;
        private string _message = "Loading...";

        public PageLoading()
        {
            _locker = new Locker();
            _locker.LockCountChanged += Locker_LockCountChanged;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }


        public Locker.Key Lock() => _locker.Lock();

        private void Locker_LockCountChanged(object? sender, LockCountChangedEventArgs e)
        {
            IsActive = e.IsLocked;
        }
    }
}
