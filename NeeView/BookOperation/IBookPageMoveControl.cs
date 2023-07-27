using System;

namespace NeeView
{
    /// <summary>
    /// ブックのページ移動操作
    /// </summary>
    public interface IBookPageMoveControl
    {
        void PrevPage(object? sender);
        void NextPage(object? sender);

        void PrevOnePage(object? sender);
        void NextOnePage(object? sender);

        void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter);
        void NextScrollPage(object? sender, ScrollPageCommandParameter parameter);

        void JumpPage(object? sender, int index);
        void JumpRandomPage(object? sender);

        void PrevSizePage(object? sender, int size);
        void NextSizePage(object? sender, int size);

        void PrevFolderPage(object? sender, bool isShowMessage);
        void NextFolderPage(object? sender, bool isShowMessage);

        void FirstPage(object? sender);
        void LastPage(object? sender);
    }


    public class DummyPageMoveControl : IBookPageMoveControl
    {
        public void FirstPage(object? sender) { }
        public void JumpPage(object? sender, int index) { }
        public void JumpRandomPage(object? sender) { }
        public void LastPage(object? sender) { }
        public void NextFolderPage(object? sender, bool isShowMessage) { }
        public void NextOnePage(object? sender) { }
        public void NextPage(object? sender) { }
        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter) { }
        public void NextSizePage(object? sender, int size) { }
        public void PrevFolderPage(object? sender, bool isShowMessage) { }
        public void PrevOnePage(object? sender) { }
        public void PrevPage(object? sender) { }
        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter) { }
        public void PrevSizePage(object? sender, int size) { }
    }
}