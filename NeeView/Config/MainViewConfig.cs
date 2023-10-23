using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    public class MainViewConfig : BindableBase
    {
        private bool _isFloating;
        private bool _isTopmost;
        private bool _isHideTitleBar;
        private bool _isAutoStretch;
        private Size _referenceSize;
        private AlternativeContent _alternativeContent = AlternativeContent.PageList;


        [PropertyMember]
        public bool IsFloating
        {
            get { return _isFloating; }
            set { SetProperty(ref _isFloating, value); }
        }

        [PropertyMember]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember]
        public bool IsHideTitleBar
        {
            get { return _isHideTitleBar; }
            set { SetProperty(ref _isHideTitleBar, value); }
        }

        /// <summary>
        /// メインビューの代替コンテンツ
        /// </summary>
        [PropertyMember]
        public AlternativeContent AlternativeContent
        {
            get { return _alternativeContent; }
            set { SetProperty(ref _alternativeContent, value); }
        }

        [PropertyMember]
        public bool IsAutoStretch
        {
            get { return _isAutoStretch; }
            set { SetProperty(ref _isAutoStretch, value); }
        }


        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;

        /// <summary>
        /// リファレンスサイズ
        /// </summary>
        [PropertyMapIgnore]
        public Size ReferenceSize
        {
            get { return _referenceSize; }
            set { SetProperty(ref _referenceSize, value); }
        }
    }


    /// <summary>
    /// メインビューの代替コンテンツ
    /// </summary>
    public enum AlternativeContent
    {
        [AliasName]
        Blank,

        [AliasName]
        PageList,
    }
}


