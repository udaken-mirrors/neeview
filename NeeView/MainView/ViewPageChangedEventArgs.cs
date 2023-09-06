using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ViewPageChangedEventArgs : EventArgs
    {
        public ViewPageChangedEventArgs(IReadOnlyList<Page> pages)
        {
            Pages = pages;
        }

        public IReadOnlyList<Page> Pages { get; }
    }
}
