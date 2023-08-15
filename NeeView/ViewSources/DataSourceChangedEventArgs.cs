using System;
using NeeView.ComponentModel;

namespace NeeView
{
    public class DataSourceChangedEventArgs : EventArgs
    {
        public DataSourceChangedEventArgs(DataSource dataSource)
        {
            DataSource = dataSource;
        }

        public DataSource DataSource { get; }
    }

}
