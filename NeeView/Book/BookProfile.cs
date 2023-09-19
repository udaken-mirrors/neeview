
using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本：設定
    /// </summary>
    public class BookProfile : BindableBase
    {
        static BookProfile() => Current = new BookProfile();
        public static BookProfile Current { get; }


        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        public bool CanPrioritizePageMove()
        {
            return Config.Current.Book.IsPrioritizePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        // 除外パス判定
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => Config.Current.Book.Excludes.ConainsOrdinalIgnoreCase(e));
        }

    }

}
