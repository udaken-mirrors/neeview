using System;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// サムネイル用インターフェイス
    /// </summary>
    public interface IThumbnail
    {
        public event EventHandler? Changed;

        bool IsValid { get; }
        bool IsUniqueImage { get; }
        bool IsNormalImage { get; }
        Brush Background { get; }

        ImageSource? CreateImageSource();
    }
}
