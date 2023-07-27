using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    public interface IBookControl : INotifyPropertyChanged
    {
        bool IsBookmark { get; }

        bool CanDeleteBook();
        void DeleteBook();
        void ReLoad();
        void ValidateRemoveFile(IEnumerable<Page> pages);

        bool CanBookmark();
        void SetBookmark(bool isBookmark);
        void ToggleBookmark();
    }
}