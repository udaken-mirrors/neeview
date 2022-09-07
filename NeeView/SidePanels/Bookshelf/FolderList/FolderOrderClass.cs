using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// フォルダーの種類によって選択できるソートの種類が異なる、そのクラス分け
    /// </summary>
    public enum FolderOrderClass
    {
        None,
        Normal,
        WithPath,
        Full,
    }

    public static class FolderOrderClassExtension
    {
        private static readonly Dictionary<FolderOrder, string> _mapFull;
        private static readonly Dictionary<FolderOrder, string> _mapWithPath;
        private static readonly Dictionary<FolderOrder, string> _mapNormal;
        private static readonly Dictionary<FolderOrder, string> _mapNone;

        static FolderOrderClassExtension()
        {
            _mapFull = AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();

            _mapWithPath = _mapFull
                .Where(e => !e.Key.IsEntryCategory())
                .ToDictionary(e => e.Key, e => e.Value);

            _mapNormal = _mapWithPath
                .Where(e => !e.Key.IsPathCategory())
                .ToDictionary(e => e.Key, e => e.Value);

            _mapNone = _mapNormal
                .Where(e => e.Key == FolderOrder.FileName)
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public static Dictionary<FolderOrder, string> GetFolderOrderMap(this FolderOrderClass self)
        {
            return self switch
            {
                FolderOrderClass.Full => _mapFull,
                FolderOrderClass.WithPath => _mapWithPath,
                FolderOrderClass.Normal => _mapNormal,
                _ => _mapNone,
            };
        }
    }

}
