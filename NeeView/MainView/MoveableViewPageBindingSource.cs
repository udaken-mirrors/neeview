using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class MoveableViewPageBindingSource : INotifyPropertyChanged
    {
        private readonly PageFrameBoxPresenter _presenter;
        private readonly IDestinationFolderOption _option;
        private readonly DestinationFolder _dummyFolder = new DestinationFolder();

        public MoveableViewPageBindingSource(PageFrameBoxPresenter presenter, IDestinationFolderOption option)
        {
            _presenter = presenter;
            _option = option;

            _presenter.ViewPageChanged += PageFrameBoxPresenter_ViewPageChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool AnyMoveableViewPages => BookOperation.Current.Control.CanMoveToFolder(_dummyFolder, _option.MultiPagePolicy);

        private void PageFrameBoxPresenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(AnyMoveableViewPages));
        }
    }
}
