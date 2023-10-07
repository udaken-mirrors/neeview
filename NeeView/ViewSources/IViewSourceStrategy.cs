﻿using NeeView.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public interface IViewSourceStrategy
    {
        bool CheckLoaded(Size size) { return true; }
        Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token);

        void Unload() { }
    }

}