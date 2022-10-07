using System;

namespace NeeView.Interop
{
    [Flags]
    public enum AppBarState : uint
    {
        /// <summary>
        /// The taskbar is in the autohide state.
        /// <summary>
        ABS_AUTOHIDE = 0x1,

        /// <summary>
        /// he taskbar is in the always-on-top state.
        /// <summary>
        ABS_ALWAYSONTOP = 0x2
    }

}
