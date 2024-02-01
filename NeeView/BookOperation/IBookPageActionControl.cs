using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ブックの現在ページに対する操作
    /// </summary>
    public interface IBookPageActionControl
    {
        bool CanDeleteFile();
        Task DeleteFileAsync();

        bool CanDeleteFile(List<Page> pages);
        Task DeleteFileAsync(List<Page> pages);

        bool CanExport();
        void Export(ExportImageCommandParameter parameter);
        void ExportDialog(ExportImageAsCommandParameter parameter);
        
        bool CanOpenFilePlace();
        void OpenFilePlace();
        
        void CopyToClipboard(CopyFileCommandParameter parameter);
        void OpenApplication(OpenExternalAppCommandParameter parameter);

        //void OpenExternalApp(object? sender, OpenExternalAppCommandParameter parameter);
        //void ExportImage(object? sender, ExportImageCommandParameter parameter);
        //void CopyFile(object? sender);
        //void CopyImage(object? sender);
        //void DeleteFile(object? sender);

        //.. ページ依存だが、ここ？
        //void MoveToChildBook(object? sender);
    }
}
