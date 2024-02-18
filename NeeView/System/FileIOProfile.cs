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
        /// ファイル除外属性
        /// </summary>
        public FileAttributes AttributesToSkip => Config.Current.System.IsHiddenFileVisible ? FileAttributes.None : FileAttributes.Hidden;


        /// <summary>
        /// ファイルは項目として有効か？
        /// </summary>
        public bool IsFileValid(FileAttributes attributes)
        {
            return (attributes & AttributesToSkip) == 0;
        }

    }
}
