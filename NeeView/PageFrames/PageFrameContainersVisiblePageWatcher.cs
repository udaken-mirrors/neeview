using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;


namespace NeeView.PageFrames
{
    //[NotifyPropertyChanged]
    public partial class PageFrameContainersVisiblePageWatcher // : INotifyPropertyChanged
    {

        private BookContext _context;
        private PageFrameContainersViewBox _viewBox;

        private PageFrameContainersCollectionRectMath _collectionRectMath;
        private PageFrameContainersLayout _layout;

        //private List<Page> _viewPages = new();
        private List<PageFrameContainer> _visibleContainers = new();


        public PageFrameContainersVisiblePageWatcher(BookContext context, PageFrameContainersViewBox viewBox, PageFrameContainersCollectionRectMath collectionRectMath, PageFrameContainersLayout layout)
        {
            _context = context;
            _viewBox = viewBox;
            _collectionRectMath = collectionRectMath;
            _layout = layout;

            // TODO: Dispose
            _layout.LayoutChanged += Layout_LayoutChanged;
        }


        //public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler<VisibleContainersChangedEventArgs>? VisibleContainersChanged;


        public List<PageFrameContainer> VisibleContainers => _visibleContainers;

        public List<Page> VisiblePages
        {
            get
            {
                return _visibleContainers
                    .Select(e => e.Content)
                    .OfType<PageFrameContent>()
                    .SelectMany(e => e.PageFrame.Elements)
                    .Select(e => e.Page)
                    .Distinct()
                    .ToList();
            }
        }

#if false
        {
            get { return _viewPages; }
            private set
            {
                if (!_viewPages.SequenceEqual(value))
                {
                    _viewPages = value;
                    RaisePropertyChanged();
                }
            }
        }
#endif

        // TODO: 変更通知はPropertyChangedではなく専用イベントで？

        private void Layout_LayoutChanged(object? sender, EventArgs e)
        {
            Update();
        }

        public void Reset()
        {
            SetVisibleContainers(new List<PageFrameContainer>());
        }


        /// <summary>
        /// ViewPages 更新
        /// TODO: 別管理じゃないか？
        /// </summary>
        private void Update()
        {
            SetVisibleContainers(_collectionRectMath.CollectViewContainers(_viewBox.Rect));
        }

        private void SetVisibleContainers(List<PageFrameContainer> visibleContainers)
        {
            if (!_visibleContainers.SequenceEqual(visibleContainers))
            {
                var direction = (GetPageRange(visibleContainers) < GetPageRange(_visibleContainers)) ? -1 : 1;
                _visibleContainers = visibleContainers;
                VisibleContainersChanged?.Invoke(this, new VisibleContainersChangedEventArgs(direction));
            }
        }

        private PageRange GetPageRange(List<PageFrameContainer> containers)
        {
            return PageRange.Marge(containers.Select(e => e.FrameRange));
        }


    }

    public class VisibleContainersChangedEventArgs : EventArgs
    {
        public VisibleContainersChangedEventArgs(int direction)
        {
            Direction = direction;
        }

        public int Direction { get; }
    }
}
