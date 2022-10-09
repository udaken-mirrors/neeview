using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView.Windows
{
    public class DpiScaleProvider : IDpiScaleProvider
    {
        public event EventHandler? DpiChanged;

        public IDisposable SubscribeDpiChanged(EventHandler handler)
        {
            DpiChanged += handler;
            return new AnonymousDisposable(() => DpiChanged -= handler);
        }


        public DpiScale DpiScale { get; private set; } = new DpiScale(1, 1);


        public bool SetDipScale(DpiScale dpi)
        {
            if (DpiScale.DpiScaleX != dpi.DpiScaleX || DpiScale.DpiScaleY != dpi.DpiScaleY)
            {
                DpiScale = dpi;
                DpiChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            else
            {
                return false;
            }
        }

        public DpiScale GetDpiScale()
        {
            return DpiScale;
        }
    }

}
