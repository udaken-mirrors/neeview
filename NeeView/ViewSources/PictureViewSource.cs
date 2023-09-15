using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeView.ComponentModel;

namespace NeeView
{
    public class PictureViewSource : ViewSource, IHasImageSource
    {
        private readonly PageContent _pageContent;
        private PictureContent _picture;

        public PictureViewSource(PageContent pageContent, IPictureSource pictureSource, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent;
            _picture = new PictureContent(_pageContent, pictureSource);
        }


        public override bool IsMemoryLocked => _pageContent.IsMemoryLocked;

        public ImageSource? ImageSource => _picture.Picture.ImageSource;


        public override bool CheckLoaded(Size size)
        {
            return IsLoaded && _picture.IsCreated(size);
        }

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            var picture = _picture.Load(data.Data, size, token);
            SetData(picture.ImageSource, picture.GetMemorySize(), null);
            await Task.CompletedTask;
        }

        public override void Unload()
        {
            if (Data is not null)
            {
                _picture.Unload();
                SetData(null, 0, null);
            }
        }
    }
}