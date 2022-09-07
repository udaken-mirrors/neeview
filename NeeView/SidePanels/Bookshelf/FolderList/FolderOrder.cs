using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び(互換用)
    /// </summary>
    [Obsolete("no used"), DataContract(Name = "FolderOrder")]
    public enum FolderOrderV1
    {
        [EnumMember]
        FileName,
        [EnumMember]
        TimeStamp,
        [EnumMember]
        Size,
        [EnumMember]
        Random,
    }

    /// <summary>
    /// フォルダーの並び
    /// </summary>
    [DataContract(Name = "FolderOrderV2")]
    public enum FolderOrder
    {
        [EnumMember]
        [AliasName]
        FileName,

        [EnumMember]
        [AliasName]
        FileNameDescending,

        [EnumMember]
        [AliasName]
        Path,

        [EnumMember]
        [AliasName]
        PathDescending,

        [EnumMember]
        [AliasName]
        FileType,

        [EnumMember]
        [AliasName]
        FileTypeDescending,

        [EnumMember]
        [AliasName]
        TimeStamp,

        [EnumMember]
        [AliasName]
        TimeStampDescending,

        [EnumMember]
        [AliasName]
        Size,

        [EnumMember]
        [AliasName]
        SizeDescending,

        [EnumMember]
        [AliasName]
        EntryTime,

        [EnumMember]
        [AliasName]
        EntryTimeDescending,

        [EnumMember]
        [AliasName]
        Random,
    }

    public static class FolderOrderExtension
    {
        [Obsolete("use legacy convert only")]
        public static FolderOrder ToV2(this FolderOrderV1 mode)
        {
            return mode switch
            {
                FolderOrderV1.TimeStamp => FolderOrder.TimeStampDescending,
                FolderOrderV1.Size => FolderOrder.SizeDescending,
                FolderOrderV1.Random => FolderOrder.Random,
                _ => FolderOrder.FileName,
            };
        }

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
