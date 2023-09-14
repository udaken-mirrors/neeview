using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageConfig : BindableBase
    {
        private bool _isMediaEnabled;
        private bool _isMediaRepeat = true;

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
