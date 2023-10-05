using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageConfig : BindableBase
    {
        private bool _isMediaRepeat = true;

        public ImageStandardConfig Standard { get; set; } = new ImageStandardConfig();

        public ImageSvgConfig Svg { get; set; } = new ImageSvgConfig();


        /// <summary>
        /// 動画ページのループフラグ
        /// </summary>
        [PropertyMember]
        public bool IsMediaRepeat
        {
            get { return _isMediaRepeat; }
            set { SetProperty(ref _isMediaRepeat, value); }
        }


    }

}
