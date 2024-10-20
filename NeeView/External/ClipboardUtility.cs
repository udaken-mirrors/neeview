using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // コピー設定
    public static class ClipboardUtility
    {
        public static async Task CopyAsync(List<Page> pages, CancellationToken token)
        {
            var data = new System.Windows.DataObject();

            if (await SetDataAsync(data, pages, Config.Current.System, token))
            {
                System.Windows.Clipboard.SetDataObject(data);
            }
        }

        public static async Task<bool> SetDataAsync(System.Windows.DataObject data, List<Page> pages, CancellationToken token)
        {
            return await SetDataAsync(data, pages, Config.Current.System, token);
        }

        /// <summary>
        /// ページのコピーデータ―を DataObject に登録する
        /// </summary>
        /// <param name="data">登録先データオブジェクト</param>
        /// <param name="pages">登録ページ</param>
        /// <param name="policy">登録方針</param>
        /// <param name="token"></param>
        /// <returns>登録成功/失敗</returns>
        private static async Task<bool> SetDataAsync(System.Windows.DataObject data, List<Page> pages, ICopyPolicy policy, CancellationToken token)
        {
            if (pages.Count == 0) return false;

            // query path
            data.SetData(pages.Select(x => new QueryPath(x.EntryFullName)).ToQueryPathCollection());

            // realize file path
            var files = await PageUtility.CreateFilePathListAsync(pages, policy.ArchiveCopyPolicy, token);
            if (files.Count > 0)
            {
                data.SetData(System.Windows.DataFormats.FileDrop, files.ToArray());
            }

            // file path text
            if (policy.TextCopyPolicy != TextCopyPolicy.None)
            {
                var paths = (policy.ArchiveCopyPolicy == ArchivePolicy.SendExtractFile && policy.TextCopyPolicy == TextCopyPolicy.OriginalPath)
                    ? await PageUtility.CreateFilePathListAsync(pages, ArchivePolicy.SendArchivePath, token)
                    : files;
                if (paths.Count > 0)
                {
                    data.SetData(System.Windows.DataFormats.UnicodeText, string.Join(System.Environment.NewLine, paths));
                }
            }

            return true;
        }

        // クリップボードに画像をコピー
        public static void CopyImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            System.Windows.Clipboard.SetImage(image);
        }

        // クリップボードからペースト(テスト)
        public static void Paste()
        {
            var data = System.Windows.Clipboard.GetDataObject(); // クリップボードからオブジェクトを取得する。
            if (data.GetDataPresent(System.Windows.DataFormats.FileDrop)) // テキストデータかどうか確認する。
            {
                var files = (string[])data.GetData(System.Windows.DataFormats.FileDrop); // オブジェクトからテキストを取得する。
                Debug.WriteLine("=> " + files[0]);
            }
        }
    }
}
