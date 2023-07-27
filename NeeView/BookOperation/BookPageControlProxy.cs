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

        public void FirstPage(object? sender)
        {
            _source?.FirstPage(sender);
        }

        public void JumpPage(object? sender, int index)
        {
            _source?.JumpPage(sender, index);
        }

        public void JumpRandomPage(object? sender)
        {
            _source?.JumpRandomPage(sender);
        }

        public void LastPage(object? sender)
        {
            _source?.LastPage(sender);
        }

        public void NextFolderPage(object? sender, bool isShowMessage)
        {
            _source?.NextFolderPage(sender, isShowMessage);
        }

        public void NextOnePage(object? sender)
        {
            _source?.NextOnePage(sender);
        }

        public void NextPage(object? sender)
        {
            _source?.NextPage(sender);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.NextScrollPage(sender, parameter);
        }

        public void NextSizePage(object? sender, int size)
        {
            _source?.NextSizePage(sender, size);
        }

        public void PrevFolderPage(object? sender, bool isShowMessage)
        {
            _source?.PrevFolderPage(sender, isShowMessage);
        }

        public void PrevOnePage(object? sender)
        {
            _source?.PrevOnePage(sender);
        }

        public void PrevPage(object? sender)
        {
            _source?.PrevPage(sender);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _source?.PrevScrollPage(sender, parameter);
        }

        public void PrevSizePage(object? sender, int size)
        {
            _source?.PrevSizePage(sender, size);
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