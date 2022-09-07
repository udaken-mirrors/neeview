using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// マーカー群表示用コレクション
    /// </summary>
    public class PageMarkerCollection
    {
        public PageMarkerCollection(List<int> indexes, int maximum)
        {
            Indexes = indexes;
            Maximum = maximum;
        }

        public List<int> Indexes { get; set; }
        public int Maximum { get; set; }
    }

    /// <summary>
    /// Pagemarkers : Model
    /// </summary>
    public class PageMarkers : BindableBase
    {
        private readonly BookOperation _bookOperation;
        private PageMarkerCollection? _markerCollection;
        private bool _isSliderDirectionReversed;


        public PageMarkers(BookOperation bookOperation)
        {
            _bookOperation = bookOperation;

            _bookOperation.BookChanged +=
                (s, e) => Update();
            _bookOperation.PagesSorted +=
                (s, e) => Update();
            _bookOperation.PageRemoved +=
                (s, e) => Update();
            _bookOperation.MarkersChanged +=
                (s, e) => Update();
        }


        /// <summary>
        /// MarkerCollection property.
        /// </summary>
        public PageMarkerCollection? MarkerCollection
        {
            get { return _markerCollection; }
            set { if (_markerCollection != value) { _markerCollection = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// スライダー方向
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            set { if (_isSliderDirectionReversed != value) { _isSliderDirectionReversed = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// マーカー更新
        /// </summary>
        private void Update()
        {
            var book = BookOperation.Current.Book;
            if (book != null && book.Marker.Markers.Any())
            {
                this.MarkerCollection = new PageMarkerCollection(
                    indexes: book.Marker.Markers.Select(e => e.Index).ToList(),
                    maximum: book.Pages.Count - 1
                );
            }
            else
            {
                this.MarkerCollection = null;
            }
        }

    }
}
