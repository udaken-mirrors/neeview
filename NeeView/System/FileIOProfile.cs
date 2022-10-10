using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        static FileIOProfile() => Current = new FileIOProfile();
        public static FileIOProfile Current { get; }


        private FileIOProfile()
        {
        }


        /// <summary>
        /// ファイルは項目として有効か？
        /// </summary>
        public bool IsFileValid(FileAttributes attributes)
        {
            return Config.Current.System.IsHiddenFileVisibled || (attributes & FileAttributes.Hidden) == 0;
        }

    }
}
