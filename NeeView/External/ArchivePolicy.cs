using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    // 圧縮ファイルの時の動作
    public enum ArchivePolicy
    {
        [AliasName]
        None,

        [AliasName]
        SendArchiveFile,

        [AliasName]
        SendArchivePath, // ver 33.0

        [AliasName]
        SendExtractFile,
    }


    public static class ArchivePolicyExtensions
    {
        public static string ToSampleText(this ArchivePolicy self)
        {
            return self switch
            {
                ArchivePolicy.None
                    => @"not run.",
                ArchivePolicy.SendArchiveFile
                    => @"C:\Archive.zip",
                ArchivePolicy.SendArchivePath
                    => @"C:\Archive.zip\File.jpg",
                ArchivePolicy.SendExtractFile
                    => @"ExtractToTempFolder\File.jpg",
                _
                    => throw new ArgumentOutOfRangeException($"not support self value: {self}"),
            };
        }
    }


    public class ArchivePolicyToSampleStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchivePolicy policy)
            {
                return Properties.TextResources.GetString("Word.Example") + ", " + policy.ToSampleText();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ArchivePolicyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchivePolicy policy)
            {
                return policy.ToAliasName();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
