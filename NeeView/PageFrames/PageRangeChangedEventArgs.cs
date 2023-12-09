using System;

namespace NeeView.PageFrames
{
    public class PageRangeChangedEventArgs : EventArgs
    {
        public static new PageRangeChangedEventArgs Empty { get; } = new(PageRange.Empty, PageRange.Empty);

        public PageRangeChangedEventArgs(PageRange newValue, PageRange oldValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }

        public PageRange NewValue { get; init; }
        public PageRange OldValue { get; init; }

        public bool IsMatchAnyEdge()
        {
            return NewValue.Min == OldValue.Min || NewValue.Max == OldValue.Max;
        }

        public override string ToString()
        {
            return $"{OldValue} => {NewValue}";
        }
    }
}
