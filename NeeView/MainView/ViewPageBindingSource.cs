using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ViewPageBindingSource : INotifyPropertyChanged
    {
        public static ViewPageBindingSource Default { get; } = new ViewPageBindingSource(PageFrameBoxPresenter.Current);


        private readonly PageFrameBoxPresenter _presenter;

        public ViewPageBindingSource(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
            _presenter.ViewPageChanged += PageFrameBoxPresenter_ViewPageChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IReadOnlyList<Page> ViewPages => _presenter.ViewPages;

        public bool AnyViewPages => ViewPages.Any();

        private void PageFrameBoxPresenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            RaisePropertyChanged("");
        }
    }
}
