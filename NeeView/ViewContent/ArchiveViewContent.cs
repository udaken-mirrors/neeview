using System;
using System.Windows;

namespace NeeView
{
    public class ArchiveViewContent : ViewContent
    {
        public ArchiveViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
        }


        public override bool IsBitmapScalingModeSupported => false;

        public override bool IsViewContent => false;


        private void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            if (this.Source is null) throw new InvalidOperationException();
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            this.Size = new Size(512, 512);
        }

        private static FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            var content = source.Content as ArchiveContent ?? throw new InvalidOperationException("Content must be ArchiveContent");
            var control = new ArchivePageControl(content);
            control.SetBinding(ArchivePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }


        public static ArchiveViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new ArchiveViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }

}
