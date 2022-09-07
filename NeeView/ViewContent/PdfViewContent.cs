using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Pdf ViewContent
    /// </summary>
    public class PdfViewContent : BitmapViewContent
    {
        private readonly BitmapContent _bitmapContent;

        public PdfViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
            _bitmapContent = this.Content as BitmapContent ?? throw new InvalidOperationException("Content must be BitmapContent");
        }


        private void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            if (this.Source is null) throw new InvalidOperationException();
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            this.Color = _bitmapContent.Color;
        }

        public override bool Rebuild(double scale)
        {
            var size = GetScaledSize(scale);
            return Rebuild(size);
        }


        public new static PdfViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new PdfViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }
}
