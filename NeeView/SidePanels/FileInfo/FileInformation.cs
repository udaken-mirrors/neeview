using NeeLaboratory.ComponentModel;
using NeeView.Windows.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FileInformation : BindableBase
    {
        public static FileInformation Current { get; }
        static FileInformation() => Current = new FileInformation();

        
        private readonly DelayValue<IEnumerable<Page>> _viewContentsDelay;
        private List<FileInformationSource>? _fileInformations;


        private FileInformation()
        {
            var mainViewComponent = MainViewComponent.Current;

            mainViewComponent.PageFrameBoxPresenter.SelectedRangeChanged +=
                (s, e) => Update(mainViewComponent.PageFrameBoxPresenter.SelectedPages);

            //mainViewComponent.ContentCanvas.ContentChanged +=
            //    (s, e) => Update(mainViewComponent.ContentCanvas.Contents);

            _viewContentsDelay = new DelayValue<IEnumerable<Page>>();
            _viewContentsDelay.ValueChanged += ViewContentsDelay_ValueChanged;
        }


        public List<FileInformationSource>? FileInformations
        {
            get { return _fileInformations; }
            set { SetProperty(ref _fileInformations, value); }
        }


        public FileInformationSource? GetMainFileInformation()
        {
            return FileInformations?.OrderBy(e => e.Page?.Index ?? int.MaxValue).FirstOrDefault();
        }

        public void Update(IEnumerable<Page> pages)
        {
            _viewContentsDelay.SetValue(pages.ToList(), 100); // 100ms delay
        }

        private void ViewContentsDelay_ValueChanged(object? sender, EventArgs _)
        {
            this.FileInformations = _viewContentsDelay.Value?
                .Reverse()
                //.Where(e => e.IsInformationValid)
                .Select(e => new FileInformationSource(e))
                .ToList();
        }

        public void Update()
        {
            if (_fileInformations is null) return;

            foreach (var item in _fileInformations)
            {
                item.Update();
            }
        }

    }

}
