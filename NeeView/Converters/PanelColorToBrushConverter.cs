﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class PanelColorToBrushConverter : IValueConverter
    {
        public Brush Dark { get; set; }
        public Brush Light { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PanelColor panelColor)
            {
                return panelColor == PanelColor.Dark ? Dark : Light;
            }
            return Dark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
