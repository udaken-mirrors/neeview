using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using System;

namespace NeeView.PageFrames
{
    public interface INotifyTransformChanged
    {
        event TransformChangedEventHandler? TransformChanged;
    }


    public enum TransformChangeSource
    {
        Logic,
        ViewControl,
    }

    public enum TransformCategory
    {
        Content,
        View,
        Loupe,
    }

    public enum TransformAction
    {
        Point,
        Angle,
        Scale,
        Flip,
    }

    public class TransformChangedEventArgs : EventArgs
    {
        public TransformChangedEventArgs(TransformCategory category, TransformAction action)
        {
            Category = category;
            Action = action;
        }

        public TransformCategory Category { get; }
        public TransformAction Action { get; }
    }

    public delegate void TransformChangedEventHandler(object? sender, TransformChangedEventArgs e);
}