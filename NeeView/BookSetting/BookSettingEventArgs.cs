using System;

namespace NeeView
{
    public class BookSettingEventArgs : EventArgs
    {
        public static new readonly BookSettingEventArgs Empty = new();

        public BookSettingEventArgs()
        {
        }

        public BookSettingEventArgs(BookSettingKey key)
        {
            Key = key;
        }

        public BookSettingKey Key { get; set; }
    }
}
