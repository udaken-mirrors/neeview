using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageControlProxy : IBookPageControl
    {
        private IBookPageControl? _source;

        public BookPageControlProxy()
        {
        }


        public void SetSource(IBookPageControl? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);
        }

        private void Attach(IBookPageControl? source)
        {
            if (_source == source) return;

            _source = source;
        }

        private void Detach()
        {
            if (_source is null) return;
        }


        #region IBookPageMoveControl

        public void MoveToFirst(object? sender)
        {
            _source?.MoveToFirst(sender);
        }

        public void MoveTo(object? sender, int index)
        {
            _source?.MoveTo(sender, index);
        }

        public void MoveToRandom(object? sender)
        {
            _source?.MoveToRandom(sender);
        }

        public void MoveToLast(object? sender)
        {
            _source?.MoveToLast(sender);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _source?.MoveNextFolder(sender, isShowMessage);
        }

        public void MoveNextOne(object? sender)
        {
            _source?.MoveNextOne(sender);
        }

        public void MoveNext(object? sender)
        {
            _source?.MoveNext(sender);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.ScrollToNextFrame(sender, parameter);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _source?.MoveNextSize(sender, size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _source?.MovePrevFolder(sender, isShowMessage);
        }

        public void MovePrevOne(object? sender)
        {
            _source?.MovePrevOne(sender);
        }

        public void MovePrev(object? sender)
        {
            _source?.MovePrev(sender);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.ScrollToPrevFrame(sender, parameter);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _source?.MovePrevSize(sender, size);
        }

        #endregion IBookPageMoveControl

        #region IBookPageActionControl

        public bool CanDeleteFile()
        {
            return _source?.CanDeleteFile() ?? false;
        }

        public bool CanExport()
        {
            return _source?.CanExport() ?? false;
        }

        public bool CanOpenFilePlace()
        {
            return _source?.CanOpenFilePlace() ?? false;
        }

        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _source?.CopyToClipboard(parameter);
        }

        public Task DeleteFileAsync()
        {
            return _source?.DeleteFileAsync() ?? Task.CompletedTask;
        }

        public void Export(ExportImageCommandParameter parameter)
        {
            _source?.Export(parameter);
        }

        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            _source?.ExportDialog(parameter);
        }

        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            _source?.OpenApplication(parameter);
        }

        public void OpenFilePlace()
        {
            _source?.OpenFilePlace();
        }

        public bool CanDeleteFile(List<Page> pages)
        {
            return _source?.CanDeleteFile(pages) ?? false;
        }

        public Task DeleteFileAsync(List<Page> pages)
        {
            return _source?.DeleteFileAsync(pages) ?? Task.CompletedTask;
        }

        #endregion IBookPageActionControl
    }

}