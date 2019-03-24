﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace NeeView
{
    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }

    public class ThemeProfile : BindableBase
    {
        static ThemeProfile() => Current = new ThemeProfile();
        public static ThemeProfile Current { get; }

        private PanelColor _panelColor = PanelColor.Dark;
        private PanelColor _menuColor = PanelColor.Light;


        private ThemeProfile()
        {
            RefreshThemeColor();
        }


        public event EventHandler ThemeColorChanged;


        // テーマカラー
        [PropertyMember("@ParamPanelColor")]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set
            {
                if (SetProperty(ref _panelColor, value))
                {
                    RefreshThemeColor();
                }
            }
        }

        /// <summary>
        /// テーマカラー：メニュー
        /// </summary>
        [PropertyMember("@ParamMenuColor")]
        public PanelColor MenuColor
        {
            get { return _menuColor; }
            set { SetProperty(ref _menuColor, value); }
        }


        public void RefreshThemeColor()
        {
            if (App.Current == null) return;

            if (PanelColor == PanelColor.Dark)
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVSuppresstBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x90, 0xEE, 0x90));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVFolderPen"] = null;
            }
            else
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xF8, 0xF8));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVSuppresstBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0x44));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                App.Current.Resources["NVFolderPen"] = new Pen(new SolidColorBrush(Color.FromRgb(0xDE, 0xB9, 0x82)), 1);
            }

            ////RefreshSliderBrushes();

            ThemeColorChanged?.Invoke(this, null);
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(PanelColor.Dark)]
            public PanelColor PanelColor { get; set; }

            [DataMember, DefaultValue(PanelColor.Light)]
            public PanelColor MenuColor { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelColor = this.PanelColor;
            memento.MenuColor = this.MenuColor;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelColor = memento.PanelColor;
            this.MenuColor = memento.MenuColor;
        }

        #endregion

    }
}