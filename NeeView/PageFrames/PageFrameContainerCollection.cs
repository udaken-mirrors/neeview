﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;


namespace NeeView.PageFrames
{
    public interface IInitializable<T>
    {
        void Initialize(T item);
        void Uninitialized(T item);
    }

    public enum PageFrameContainerCollectionChangedEventAction
    {
        Add,
        Update,
        UpdateTransform, // view control
        UpdateContentSize, // loaded
        UpdateContainerLayout,
        Remove,
    }

    public class PageFrameContainerCollectionChangedEventArgs : EventArgs
    {
        public PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction action, LinkedListNode<PageFrameContainer> node)
        {
            Debug.Assert(action != PageFrameContainerCollectionChangedEventAction.UpdateTransform);
            Action = action;
            Node = node;
        }

        public PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction action, LinkedListNode<PageFrameContainer> node, TransformChangedEventArgs transformChangedEventArgs)
        {
            Debug.Assert(action == PageFrameContainerCollectionChangedEventAction.UpdateTransform);
            Action = action;
            Node = node;

            if (node.Value.Content is PageFrameContent item)
            {
                var frame = item.PageFrame.Scale;
                var elementScale = item.PageFrame.Elements.First().Scale;
                TransformChangedEventArgs = new OriginalScaleTransformChangedEventArgs(transformChangedEventArgs, frame * elementScale);
            }
        }

        public PageFrameContainerCollectionChangedEventAction Action { get; }
        public LinkedListNode<PageFrameContainer> Node { get; }
        public OriginalScaleTransformChangedEventArgs? TransformChangedEventArgs { get; }
    }


    /// <summary>
    /// PageFrameContainer のコレクションを管理
    /// </summary>
    public partial class PageFrameContainerCollection : IEnumerable<PageFrameContainer>
    {
        private readonly LinkedList<PageFrameContainer> _containers = new();
        private readonly PageFrameContext _context;
        private readonly PageFrameFactory _frameFactory;
        private readonly PageFrameContainerFactory _containerFactory;
        private readonly PageFrameContainerAnchor _anchor;
        private IInitializable<PageFrameContainer>? _containerInitializer;


        public PageFrameContainerCollection(PageFrameContext context, PageFrameFactory frameFactory, PageFrameContainerFactory containerFactory)
        {
            _context = context;
            _frameFactory = frameFactory;
            _containerFactory = containerFactory;

            var firstActivity = new PageFrameActivity();
            FirstTerminate = new PageFrameContainer(new TerminalPageFrameContent(_frameFactory.GetFirstTerminalRange(), firstActivity), firstActivity, _context.ViewScrollContext);
            _containers.AddFirst(FirstTerminate);

            var lastActivity = new PageFrameActivity();
            LastTerminate = new PageFrameContainer(new TerminalPageFrameContent(_frameFactory.GetLastTerminalRange(), lastActivity), lastActivity, _context.ViewScrollContext);
            _containers.AddLast(LastTerminate);

            _anchor = new PageFrameContainerAnchor(this);

            // [DEV]
            //this.CollectionChanged += (s, e) =>
            //{
            //    Debug.WriteLine($"CollectionChanged: {e.Action}, {e.Node.Value}");
            //};
        }


        [Subscribable]
        public event EventHandler<PageFrameContainerCollectionChangedEventArgs>? CollectionChanging;

        [Subscribable]
        public event EventHandler<PageFrameContainerCollectionChangedEventArgs>? CollectionChanged;


        public PageFrameContainerAnchor Anchor => _anchor;

        public PageFrameContainer FirstTerminate { get; }
        public PageFrameContainer LastTerminate { get; }


        public IEnumerator<PageFrameContainer> GetEnumerator()
        {
            return ((IEnumerable<PageFrameContainer>)_containers).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_containers).GetEnumerator();
        }


        public IEnumerable<PageFrameContainer> Collect<T>()
                where T : IPageFrameContent
        {
            return _containers.Where(e => e.Content is T);
        }

        public IEnumerable<LinkedListNode<PageFrameContainer>> CollectNode(LinkedListDirection direction = LinkedListDirection.Next)
        {
            return _containers.GetNodeEnumerable(direction);
        }

        public IEnumerable<LinkedListNode<PageFrameContainer>> CollectNode<T>(LinkedListDirection direction = LinkedListDirection.Next)
            where T : IPageFrameContent
        {
            return _containers.GetNodeEnumerable(direction).Where(e => e.Value.Content is T);
        }


        public void SetContainerInitializer(IInitializable<PageFrameContainer>? containerInitializer)
        {
            foreach (var container in _containers)
            {
                _containerInitializer?.Uninitialized(container);
            }

            _containerInitializer = containerInitializer;

            foreach (var container in _containers)
            {
                _containerInitializer?.Initialize(container);
            }
        }


        public bool Contains(PageFrameContainer container)
        {
            return _containers.Contains(container);
        }

        public bool ContainsNode(LinkedListNode<PageFrameContainer> node)
        {
            return node.List == _containers;
        }


        public LinkedListNode<PageFrameContainer>? Find(PageFrameContainer container)
        {
            return _containers.Find(container);
        }

        public LinkedListNode<PageFrameContainer>? Find(PagePosition position)
        {
            return CollectNode<PageFrameContent>()
                .FirstOrDefault(e => e.Value.Identifier == position);
        }

        public LinkedListNode<PageFrameContainer>? Find(PagePosition position, LinkedListDirection direction)
        {
            return CollectNode<PageFrameContent>(direction)
                .FirstOrDefault(e => e.Value.FrameRange.Top(direction.ToSign()) == position);
        }

        // TODO: 座標に関係するものはここで実装しないほうが良い？
        public LinkedListNode<PageFrameContainer>? Find(Point point, PageFrameOrientation orientation)
        {
            var comparer = new PointToContainerDistanceComparer(orientation, point);

            return CollectNode<PageFrameContent>()
                .MinBy(e => e.Value, comparer);
        }



        /// <summary>
        /// 最新コンテナの確保。すべてのコンテナ生成はここで行われる。
        /// </summary>
        /// <param name="position">フレーム位置</param>
        /// <param name="direction">フレーム方向</param>
        /// <returns>確保したコンテナ</returns>
        public LinkedListNode<PageFrameContainer>? EnsureLatestContainerNode(PagePosition position, LinkedListDirection direction)
        {
            var node = Find(position, direction);
            if (node is not null)
            {
                UpdateContainerNode(node, direction);
            }
            else
            {
                // create
                node = CreateContainerNode(position, direction);
                // TODO: コンテナの衝突による削除でアンカーが変更され未配置状態の自身がアンカーになる時がある。どう回避する？
                // TODO: たとえばコンテナにアンカーフラグを保持し、衝突時にはアンカーのコンテナ座標を反映させるとか？
                // TODO: 新規コンテナはすべてアンカーと同じ座標に設定しておくとか？
                if (node is not null)
                {
                    // TODO: これアンカー実装では？
                    node.Value.Center = _anchor.Node.Value.Center;
                }
            }

            return node;
        }

        public LinkedListNode<PageFrameContainer>? EnsureLatestContainerNode(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            if (node.Value.Content is not PageFrameContent) return node;
            return EnsureLatestContainerNode(node.Value.FrameRange.Top(direction.ToSign()), direction);
        }



        /// <summary>
        /// コンテナの作成・更新 ... 機能重複はよろしくない
        /// </summary>
        /// <param name="pos">フレーム位置</param>
        /// <param name="direction">フレーム方向</param>
        /// <returns>作られたコンテナノード。作成出来ない場合は null を返す</returns>
        public LinkedListNode<PageFrameContainer>? CreateContainerNode(PagePosition pos, LinkedListDirection direction)
        {
            var frame = _frameFactory.CreatePageFrame(pos, direction.ToSign());
            if (frame is null)
            {
                return null;
            }

            var node = CollectNode(direction).FirstOrDefault(e => e.Value.FrameRange.Top(direction.ToSign()) == pos);

            // 対応するコンテナが存在するときにはエラー
            if (node is not null) throw new ArgumentException("The target container already exists.");

            var container = _containerFactory.Create(frame);
            _containerInitializer?.Initialize(container);
            node = new LinkedListNode<PageFrameContainer>(container);
            CollectionChanging?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Add, node));
            AddContainerNode(node);
            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Add, node));
            RemoveConflictContainer(node);

            return node;
        }

        public void UpdateContainerNode(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            Debug.Assert(node.Value.Content is PageFrameContent);
            var pos = node.Value.FrameRange.Top(direction.ToSign());
            var frame = _frameFactory.CreatePageFrame(pos, direction.ToSign());
            Debug.Assert(frame is not null);

            if (!node.Value.IsDirty && node.Value.Content is PageFrameContent item && item.PageFrame == frame)
            {
                return;
            }

            CollectionChanging?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Update, node));
            _containerFactory.Update(node.Value, frame);
            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Update, node));

            RemoveConflictContainer(node);
        }


        /// <summary>
        /// コンテナをコレクションに追加。
        /// ページ順になるよう挿入される
        /// </summary>
        /// <param name="node">追加するコンテナ</param>
        private void AddContainerNode(LinkedListNode<PageFrameContainer> node)
        {
            var targetNode = _containers.First;

            node.Value.TransformChanged += Container_TransformChanged;
            node.Value.ContentSizeChanged += Container_ContentSizeChanged;
            node.Value.ContainerLayoutChanged += Container_ContainerLayoutChanged;

            while (targetNode != null)
            {
                if (node.Value.Identifier < targetNode.Value.Identifier)
                {
                    _containers.AddBefore(targetNode, node);
                    return;
                }
                targetNode = targetNode.Next;
            }

            _containers.AddLast(node);
        }

        private void Container_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            if (sender is not PageFrameContainer container) return;

            var node = Find(container);
            if (node is null) return;

            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.UpdateTransform, node, e));
        }

        private void Container_ContentSizeChanged(object? sender, EventArgs e)
        {
            if (sender is not PageFrameContainer container) return;

            var node = Find(container);
            if (node is null) return;

            // TODO: ここでコンテナのコンテンツ再生成しちゃう？

            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.UpdateContentSize, node));
        }

        private void Container_ContainerLayoutChanged(object? sender, EventArgs e)
        {
            if (sender is not PageFrameContainer container) return;

            var node = Find(container);
            if (node is null) return;

            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.UpdateContainerLayout, node));
        }



        /// <summary>
        /// 衝突しているコンテナを削除
        /// </summary>
        /// <param name="container"></param>
        private void RemoveConflictContainer(LinkedListNode<PageFrameContainer> anchor)
        {
            RemoveConflictContainer(anchor, LinkedListDirection.Previous);
            RemoveConflictContainer(anchor, LinkedListDirection.Next);
        }

        private void RemoveConflictContainer(LinkedListNode<PageFrameContainer> anchor, LinkedListDirection direction)
        {
            var node = anchor.GetNext(direction);
            while (node is not null && node.Value.FrameRange.Confrict(anchor.Value.FrameRange))
            {
                var next = node.GetNext(direction);
                if (!node.Value.IsLocked)
                {
                    RemoveContainerNode(node);
                }
                node = next;
            }
        }


        /// <summary>
        /// コンテナ削除
        /// </summary>
        /// <param name="node">削除開始コンテナ</param>
        /// <param name="direction">削除方向</param>
        public void RemoveContainers(LinkedListNode<PageFrameContainer>? node, LinkedListDirection direction)
        {
            if (node is null) return;

            while (node is not null)
            {
                var next = node.GetNext(direction);
                if (!node.Value.IsLocked)
                {
                    RemoveContainerNode(node);
                }
                node = next;
            }
        }

        /// <summary>
        /// コンテナの削除
        /// </summary>
        /// <param name="node"></param>
        public void RemoveContainerNode(LinkedListNode<PageFrameContainer> node)
        {
            Debug.Assert(!node.Value.IsLocked);

            CollectionChanging?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Remove, node));
            _containers.Remove(node);
            _containerInitializer?.Uninitialized(node.Value);
            node.Value.TransformChanged -= Container_TransformChanged;
            node.Value.ContentSizeChanged -= Container_ContentSizeChanged;
            node.Value.ContainerLayoutChanged -= Container_ContainerLayoutChanged;
            node.Value.Dispose();
            CollectionChanged?.Invoke(this, new PageFrameContainerCollectionChangedEventArgs(PageFrameContainerCollectionChangedEventAction.Remove, node));
        }

        /// <summary>
        /// コンテナをすべて削除
        /// </summary>
        public void Clear()
        {
            RemoveContainers(_containers.First, LinkedListDirection.Next);
        }

        /// <summary> 
        /// すべてのコンテナに作り直しフラグ設定
        /// </summary>
        public void SetDirty(PageFrameDirtyLevel level)
        {
            foreach (var container in _containers.ToList())
            {
                container.DirtyLevel = level;
            }
        }

        public void UpdateContainer(LinkedListNode<PageFrameContainer> node)
        {
            if (node.Value.Content is not PageFrameContent) return;
            if (!node.Value.IsDirty) return;

            var direction = GetContainerDirection(node);
            var position = node.Value.FrameRange.Top(direction.ToSign());

            var newer = EnsureLatestContainerNode(position, direction);
            Debug.Assert(newer is not null && newer.Value.CompareTo(node.Value) == 0);
        }

        // TODO: これアンカー実装では？
        private LinkedListDirection GetContainerDirection(LinkedListNode<PageFrameContainer> node)
        {
            return node == Anchor.Node
                ? Anchor.Direction
                : node.Value.Identifier < Anchor.Container.Identifier ? LinkedListDirection.Previous : LinkedListDirection.Next;
        }
    }
}