using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ先読み
    /// </summary>
    public class BookAhead : BindableBase, IDisposable
    {
        private readonly PageContentJobClient _jobClient = new("Ahead", JobCategories.PageAheadContentJobCategory);
        private readonly BookMemoryService _bookMemoryService;
        private bool _isBusy;
        private List<Page>? _pages;
        private int _index;
        private Page? _page;
        private readonly object _lock = new();



        public BookAhead(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }



        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public void Order(List<Page> pages)
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _pages = pages;
                _index = 0;
                _page = null;

                IsBusy = LoadNext();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pages = null;
                _index = 0;
                _page = null;
            }
        }

        /// <summary>
        /// ページ読み込み完了を通知して次の先読みを行う
        /// </summary>
        public void OnPageLoaded(object sender, PageChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (e.Page != _page) return;

            if (_bookMemoryService.IsFull)
            {
                IsBusy = false;
                return;
            }

            IsBusy = LoadNext();
        }

        private bool LoadNext()
        {
            if (_disposedValue) return false;

            lock (_lock)
            {
                do
                {
                    if (_pages is null || _index >= _pages.Count)
                    {
                        return false;
                    }
                    _page = _pages[_index];
                    _page.State = PageContentStateExtension.Max(_page.State, PageContentState.Ahead);
                    _index++;
                }
                while (_page.IsLoaded);

                _jobClient.Order(new List<Page>() { _page });
                return true;
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Clear();
                    ResetPropertyChanged();
                    _jobClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
