using NeeLaboratory.ComponentModel;
using System;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// NowLoading : ViewModel
    /// </summary>
    public class NowLoadingViewModel : BindableBase
    {
        private NowLoading _model;


        public NowLoadingViewModel(NowLoading model)
        {
            _model = model;

            _model.SubscribePropertyChanged(nameof(_model.IsDispNowLoading),
                (_, _) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsDispNowLoading))));
        }


        public bool IsDispNowLoading
        {
            get => _model.IsDispNowLoading;
            set => _model.IsDispNowLoading = value;
        }
    }

}
