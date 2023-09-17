using Jint.Native;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{

    public partial class BookPageMoveControl : IBookPageMoveControl
    {
        private readonly PageFrameBox _box;


        public BookPageMoveControl(PageFrameBox box)
        {
            _box = box;
        }



        public IReadOnlyList<Page> Pages => _box.Pages;

        public PageRange SelectedRange => _box.SelectedRange;


        public void MovePrev(object? sender)
        {
            _box.MoveToNextFrame(LinkedListDirection.Previous);
        }

        public void MoveNext(object? sender)
        {
            _box.MoveToNextFrame(LinkedListDirection.Next);
        }

        public void MovePrevOne(object? sender)
        {
            _box.MoveToNextPage(LinkedListDirection.Previous);
        }

        public void MoveNextOne(object? sender)
        {
            _box.MoveToNextPage(LinkedListDirection.Next);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _box.ScrollToNextFrame(LinkedListDirection.Previous, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _box.ScrollToNextFrame(LinkedListDirection.Next, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void MoveTo(object? sender, int index)
        {
            _box.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MoveToRandom(object? sender)
        {
            var random = new Random();
            var index = random.Next(Pages.Count);
            _box.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _box.MoveTo(new PagePosition(SelectedRange.Min.Index - size, 0), LinkedListDirection.Previous);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _box.MoveTo(new PagePosition(SelectedRange.Min.Index + size, 0), LinkedListDirection.Next);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _box.MoveToNextFolder(LinkedListDirection.Previous, isShowMessage);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _box.MoveToNextFolder(LinkedListDirection.Next, isShowMessage);
        }

        public void MoveToFirst(object? sender)
        {
            _box.MoveTo(new PagePosition(0, 0), LinkedListDirection.Next);
        }

        public void MoveToLast(object? sender)
        {
            _box.MoveTo(new PagePosition(Pages.Count - 1, 0), LinkedListDirection.Next);
        }
    }

}