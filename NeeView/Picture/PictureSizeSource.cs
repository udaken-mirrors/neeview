using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 画像生成サイズパラメータ
    /// </summary>
    /// <remarks>
    /// サイズと設定ハッシュのセット。同じ画像であることの確認用
    /// </remarks>
    public record class PictureSizeSource
    {
        public PictureSizeSource() : this(new Size(0, 0), 0, true)
        {
        }

        public PictureSizeSource(Size size, int filterHashCode, bool isKeepAspectRatio)
        {
            Size = size;
            FilterHashCode = filterHashCode;
            IsKeepAspectRatio = isKeepAspectRatio;
        }

        public Size Size { get; }
        public int FilterHashCode { get; }
        public bool IsKeepAspectRatio { get; }
    }
}
