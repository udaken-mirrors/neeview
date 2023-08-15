using NeeLaboratory.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public class WindowTitle : BindableBase
    {
        static WindowTitle() => Current = new WindowTitle();
        public static WindowTitle Current { get; }


        private readonly string _defaultWindowTitle;
        private string _title = "";

        private readonly BookHub _bookHub;
        private readonly MainViewComponent _mainViewComponent;
        private readonly TitleStringService _titleStringService;
        private readonly TitleString _titleString;


        public WindowTitle()
        {
            _bookHub = BookHub.Current;
            _mainViewComponent = MainViewComponent.Current;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _defaultWindowTitle = $"{Environment.ApplicationName} {Environment.DispVersion}";

#warning not implement yet
#if false
            _mainViewComponent.ContentCanvas.ContentChanged += (s, e) =>
            {
                UpdateFormat();
            };
#endif

            _bookHub.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.WindowTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(WindowTitleConfig.WindowTitleFormat1):
                    case nameof(WindowTitleConfig.WindowTitleFormat2):
                    case nameof(WindowTitleConfig.WindowTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
        }


        private void TitleString_TitleChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateFormat()
        {
#if false
            var contents = _mainViewComponent.ContentCanvas.CloneContents;
            var mainContent = _mainViewComponent.ContentCanvas.MainContent;
            var subContent = contents.First(e => e != mainContent);

            string format = mainContent is MediaViewContent
                ? Config.Current.WindowTitle.WindowTitleFormatMedia
                : subContent.IsValid && !subContent.IsDummy ? Config.Current.WindowTitle.WindowTitleFormat2 : Config.Current.WindowTitle.WindowTitleFormat1;

            _titleString.SetFormat(format);
#endif
        }

        private void UpdateTitle()
        {
            var address = _bookHub.GetCurrentBook()?.Path;

            if (_bookHub.IsLoading)
            {
                Title = LoosePath.GetFileName(_bookHub.LoadingPath) + " " + Properties.Resources.Notice_LoadingTitle;
            }
            else if (address == null)
            {
                Title = _defaultWindowTitle;
            }
#if false
            else if (_mainViewComponent.ContentCanvas.MainContent?.Source == null)
            {
                Title = LoosePath.GetDispName(address);
            }
#endif
            else
            {
                Title = _titleString.Title;
            }
        }

    }
}
