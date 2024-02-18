using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NeeView
{
    public static class DestinationFolderCollectionUtility
    {
        /// <summary>
        /// 「フォルダーにコピー」「フォルダーに移動」メニュー作成
        /// </summary>
        /// <param name="title">メニュータイトル</param>
        /// <param name="isEnabled">メニューの有効/無効</param>
        /// <param name="command">実行コマンド</param>
        /// <param name="OpenDestinationFolderDialogCommand">設定コマンド</param>
        public static MenuItem CreateDestinationFolderItem(string title, bool isEnabled, ICommand command, ICommand OpenDestinationFolderDialogCommand)
        {
            var subItem = new MenuItem() { Header = title, IsEnabled = isEnabled };

            if (Config.Current.System.DestinationFolderCollection.Any())
            {
                for (int i = 0; i < Config.Current.System.DestinationFolderCollection.Count; ++i)
                {
                    var folder = Config.Current.System.DestinationFolderCollection[i];
                    var header = new TextBlock(new Run(folder.Name));
                    subItem.Items.Add(new MenuItem() { Header = header, ToolTip = folder.Path, Command = command, CommandParameter = folder });
                }
            }
            else
            {
                subItem.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("Word.ItemNone"), IsEnabled = false });
            }

            subItem.Items.Add(new Separator());
            subItem.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("BookshelfItem.Menu.DestinationFolderOption"), Command = OpenDestinationFolderDialogCommand });

            return subItem;
        }


    }
}
