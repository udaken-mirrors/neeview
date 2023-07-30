using System;

namespace NeeView
{
    /// <summary>
    /// ページに関する操作
    /// </summary>
    public interface IBookPageControl : IBookPageContext, IBookPageMoveControl, IBookPageActionControl, IDisposable
    {
    }
}