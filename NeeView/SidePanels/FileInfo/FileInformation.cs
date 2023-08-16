using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.PageFrames;
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


        private readonly DelayValue<ViewContentChangedEventArgs> _viewContentsDelay;
        private List<FileInformationSource>? _fileInformations;


        private FileInformation()
        {
            var mainViewComponent = MainViewComponent.Current;

            mainViewComponent.PageFrameBoxPresenter.ViewContentChanged +=
                (s, e) => Update(e);

            _viewContentsDelay = new DelayValue<ViewContentChangedEventArgs>();
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

        public void Update(ViewContentChangedEventArgs e)
        {
            _viewContentsDelay.SetValue(e, 100); // 100ms delay
        }

        private void ViewContentsDelay_ValueChanged(object? sender, EventArgs _)
        {
            var pageFrameContent = _viewContentsDelay.Value?.PageFrameContent;

            var viewContents = pageFrameContent?.ViewContents ?? new List<ViewContent>();
            var direction = pageFrameContent?.ViewContentsDirection ?? 1;

            this.FileInformations = viewContents
                .Direction(direction)
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
