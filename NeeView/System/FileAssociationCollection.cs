using NeeView.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class FileAssociationCollection : List<FileAssociation>
    {
        public void Add(string extension, FileAssociationCategory category, string? description = null)
        {
            var association = new FileAssociation(extension, category) { Description = description };
            this.Add(association);
        }

        public bool TryAdd(string extension, FileAssociationCategory category, string? description = null)
        {
            if (this.Any(e => e.Extension == extension)) return false;
            Add(extension, category, description);
            return true;
        }

        /// <summary>
        /// ファイル関連付け変更をシェルに通知してアイコン表示を更新する
        /// </summary>
        public void RefreshShellIcons()
        {
            Debug.WriteLine($"FileAssociate: Refresh shell icons.");
            NativeMethods.SHChangeNotify(SHChangeNotifyEvents.SHCNE_ASSOCCHANGED, SHChangeNotifyFlags.SHCNF_IDLIST, 0, 0);
        }
    }

}