﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListView : UserControl
    {
        //
        private PageListViewModel _vm;

        //
        public PageListView()
        {
            InitializeComponent();
        }

        // constructor
        public PageListView(PageList model) : this()
        {
            _vm = new PageListViewModel(model);
            this.DockPanel.DataContext = _vm;
        }
    }

    public enum PageNameFormat
    {
        [AliasName("@EnumPageNameFormatSmart")]
        Smart,

        [AliasName("@EnumPageNameFormatNameOnly")]
        NameOnly,

        [AliasName("@EnumPageNameFormatRaw")]
        Raw,
    }


    /// <summary>
    /// 
    /// </summary>
    public class PageNameConverter : IValueConverter
    {
        public Style SmartTextStyle { get; set; }
        public Style DefaultTextStyle { get; set; }
        public Style NameOnlyTextStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var format = (PageNameFormat)value;
                switch (format)
                {
                    default:
                    case PageNameFormat.Raw:
                        return DefaultTextStyle;
                    case PageNameFormat.Smart:
                        return SmartTextStyle;
                    case PageNameFormat.NameOnly:
                        return NameOnlyTextStyle;
                }
            }
            catch { }

            return DefaultTextStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}