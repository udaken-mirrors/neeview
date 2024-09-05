using NeeView.Windows.Property;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class OpenExternalAppAsCommandParameter : CommandParameter
    {
        private int _index;

        /// <summary>
        /// 選択された外部アプリの番号。0 は未選択
        /// </summary>
        [PropertyMember(NoteConverter = typeof(IntToExternalAppString))]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, Math.Max(0, value)); }
        }
    }


    public class IntToExternalAppString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index) return "";

            if (index <= 0) return ResourceService.GetString("@OpenExternalAppAsCommandParameter.Index.Menu");
            index--;

            var items = Config.Current.System.ExternalAppCollection;
            if (items.Count <= index) return ResourceService.GetString("@OpenExternalAppAsCommandParameter.Index.Undefined");

            return Config.Current.System.ExternalAppCollection[index].DispName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
