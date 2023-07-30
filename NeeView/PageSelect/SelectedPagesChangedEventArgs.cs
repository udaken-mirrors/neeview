using System;
using System.Collections.Generic;

namespace NeeView
{
    public class SelectedPagesChangedEventArgs : EventArgs
    {
        public SelectedPagesChangedEventArgs(List<Page> pages)
        {
            Pages = pages;
        }

        public List<Page> Pages { get; }

    }
}

