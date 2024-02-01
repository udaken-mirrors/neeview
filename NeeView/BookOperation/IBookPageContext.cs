using NeeView.PageFrames;
using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 本を構成するページと選択ページ
    /// </summary>
    public interface IBookPageContext
    {
        event EventHandler? PagesChanged;
        event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        IReadOnlyList<Page> Pages { get; }
        IReadOnlyList<Page> SelectedPages { get; }
        PageRange SelectedRange { get; }
    }
}
