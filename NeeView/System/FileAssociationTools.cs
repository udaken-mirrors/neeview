using NeeView.Interop;
using System.Diagnostics;

namespace NeeView
{
    public static class FileAssociationTools
    {
        /// <summary>
        /// すべての関連づけを解除する
        /// </summary>
        public static void UnassociateAll()
        {
            var collection = FileAssociationCollectionFactory.Create(FileAssociationCollectionCreateOptions.OnlyRegistered);
            foreach(var item in collection)
            {
                item.IsEnabled = false;
            }
        }

        /// <summary>
        /// ファイル関連付け変更をシェルに通知してアイコン表示を更新する
        /// </summary>
        public static void RefreshShellIcons()
        {
            Debug.WriteLine($"FileAssociate: Refresh shell icons.");
            NativeMethods.SHChangeNotify(SHChangeNotifyEvents.SHCNE_ASSOCCHANGED, SHChangeNotifyFlags.SHCNF_IDLIST, 0, 0);
        }
    }
}