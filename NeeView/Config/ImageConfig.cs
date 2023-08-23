using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageConfig : BindableBase
    {
        private bool _isMediaEnabled;

        public ImageStandardConfig Standard { get; set; } = new ImageStandardConfig();

        public ImageSvgConfig Svg { get; set; } = new ImageSvgConfig();


        /// <summary>
        /// 動画を画像ページとする
        /// </summary>
        /// <remarks>
        /// 対応動画フォーマットは <see cref="MediaArchiveConfig.SupportFileTypes"/> に依存
        /// </remarks>
        [PropertyMember]
        public bool IsMediaEnabled
        {
            get { return _isMediaEnabled; }
            set { SetProperty(ref _isMediaEnabled, value); }
        }

    }

}
