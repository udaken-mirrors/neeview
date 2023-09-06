using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    // TODO: いろいろ精査
    public partial interface IBook
    {
        [Subscribable]
        public event EventHandler? PagesChanged;

        public BookMemoryService BookMemoryService { get; }

        public string Path { get; }

        public IReadOnlyList<Page> Pages { get; }

        public BookMemento Memento { get; }
    }

#if false
    public partial class EmptyBook : IBook
    {
        [Subscribable]
        public event EventHandler? PagesChanged;

        public BookMemoryService BookMemoryService { get; } = new BookMemoryService();

        public string Path => "";

        public IReadOnlyList<Page> Pages { get; } = new List<Page>();

        public BookMemento Memento { get; } = new BookMemento();
    }
#endif
}