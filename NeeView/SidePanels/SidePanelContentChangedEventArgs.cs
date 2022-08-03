using System;

namespace NeeView
{
    public class SidePanelContentChangedEventArgs : EventArgs
    {
        public SidePanelContentChangedEventArgs(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
    }
}
