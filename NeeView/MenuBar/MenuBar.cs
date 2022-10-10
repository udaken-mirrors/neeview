using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// MenuBar : Model
    /// </summary>
    public class MenuBar : BindableBase
    {
        private readonly WindowStateManager _windowStateManager;


        public MenuBar(WindowStateManager windowStateManager)
        {
            _windowStateManager = windowStateManager;

            NeeView.MainMenu.Current.AddPropertyChanged(nameof(NeeView.MainMenu.Menu),
                (s, e) => RaisePropertyChanged(nameof(MainMenu)));
        }


        public Menu? MainMenu => NeeView.MainMenu.Current.Menu;

        public WindowStateManager WindowStateManager => _windowStateManager;

    }
}
