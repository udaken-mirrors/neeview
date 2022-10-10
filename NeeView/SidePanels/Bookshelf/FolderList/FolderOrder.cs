using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び
    /// </summary>
    public enum FolderOrder
    {
        [AliasName]
        FileName,

        [AliasName]
        FileNameDescending,

        [AliasName]
        Path,

        [AliasName]
        PathDescending,

        [AliasName]
        FileType,

        [AliasName]
        FileTypeDescending,

        [AliasName]
        TimeStamp,

        [AliasName]
        TimeStampDescending,

        [AliasName]
        Size,

        [AliasName]
        SizeDescending,

        [AliasName]
        EntryTime,

        [AliasName]
        EntryTimeDescending,

        [AliasName]
        Random,
    }

    public static class FolderOrderExtension
    {
        public static bool IsEntryCategory(this FolderOrder mode)
        {
            return mode switch
            {
                FolderOrder.EntryTime or FolderOrder.EntryTimeDescending => true,
                _ => false,
            };
        }

        public static bool IsPathCategory(this FolderOrder mode)
        {
            return mode switch
            {
                FolderOrder.Path or FolderOrder.PathDescending => true,
                _ => false,
            };
        }
    }

}
