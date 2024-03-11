using System;
using System.Windows;

namespace NeeView.Windows
{
    public static class IDataObjectExtensions
    {
        public static T? GetData<T>(this IDataObject data)
            where T : class
        {
            try
            {
                return data.GetData(typeof(T)) as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T? GetData<T>(this IDataObject data, string format)
            where T : class
        {
            try
            {
                return data.GetData(format) as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string[] GetFileDrop(this IDataObject data)
        {
            return (string[])data.GetData(DataFormats.FileDrop, false);
        }
    }

}
