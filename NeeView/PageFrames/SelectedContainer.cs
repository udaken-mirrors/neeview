using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class SelectedContainer : INotifyPropertyChanged, IDisposable
    {
        private PageFrameContainerCollection _containers;
        private Func<LinkedListNode<PageFrameContainer>> _selectFunc;
        private LinkedListNode<PageFrameContainer> _node;
        private bool _disposedValue;


        public SelectedContainer(PageFrameContainerCollection containers, Func<LinkedListNode<PageFrameContainer>> selectFunc)
        {
            _containers = containers;
            _containers.CollectionChanged += Containers_CollectionChanged;

            _selectFunc = selectFunc;

            _node = _containers.CollectNode().First();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? IsDirtyChanged;

        [Subscribable]
        public event EventHandler<ViewContentChangedEventArgs>? ViewContentChanged;


        public LinkedListNode<PageFrameContainer> Node => _node;

        public PageFrameContainer Container => _node.Value;

        public Page? Page => (_node.Value.Content as PageFrameContent)?.PageFrame.Elements[0].Page;

        public PageRange PageRange => _node.Value.FrameRange;
        public PagePosition PagePosition => _node.Value.Identifier;

        public bool IsValid => _node.Value.Content is PageFrameContent;


        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                    _containers.CollectionChanging -= Containers_CollectionChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
        private void Containers_CollectionChanged(object? sender, PageFrameContainerCollectionChangedEventArgs e)
        {
            // 選択ノードが削除されたときの再選出
            if (e.Node == _node && e.Action == PageFrameContainerCollectionChangedEventAction.Remove)
            {
                SetAuto();
            }
        }


        public void Set(LinkedListNode<PageFrameContainer> node)
        {
            if (_disposedValue) return;

            if (_node == node) return;
            Debug.Assert(node?.Value.Content is PageFrameContent);

            Detach();
            _node = node;
            Attach();
            RaisePropertyChanged(nameof(Node));
            RaisePropertyChanged(nameof(Container));
            RaisePropertyChanged(nameof(Page));
            RaisePropertyChanged(nameof(PageRange));
            RaisePropertyChanged(nameof(PagePosition));
        }

        public void SetAuto()
        {
            if (_disposedValue) return;
            if (_selectFunc is null) return;

            Set(_selectFunc.Invoke());
        }


        private void Attach()
        {
            _node.Value.ContentChanged += Container_ContentChanged;
            _node.Value.ViewContentChanged += Container_ViewContentChanged;
            _node.Value.Activity.IsSelected = true;

            if (_node.Value.Content is PageFrameContent content)
            {
                ViewContentChanged?.Invoke(this, new ViewContentChangedEventArgs(content));
            }
        }


        private void Detach()
        {
            if (_node is null) return;

            _node.Value.ContentChanged -= Container_ContentChanged;
            _node.Value.ViewContentChanged -= Container_ViewContentChanged;
            _node.Value.Activity.IsSelected = false;
        }

        private void Container_ContentChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(PagePosition));
        }

        private void Container_ViewContentChanged(object? sender, ViewContentChangedEventArgs e)
        {
            ViewContentChanged?.Invoke(this, e);
        }


    }


}