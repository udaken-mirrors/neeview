using System;

namespace NeeView
{
    public interface IDisposableContent : IDisposable
    {
        object? Content { get; }
    }
}
