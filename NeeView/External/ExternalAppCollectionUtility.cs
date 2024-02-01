using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NeeView
{
    public static class ExternalAppCollectionUtility
    {
        /// <summary>
        /// 「外部アプリで開く」メニュー作成
        /// </summary>
        /// <param name="isEnabled">メニューの有効/無効</param>
        /// <param name="command">実行コマンド</param>
        /// <param name="OpenExternalAppDialogCommand">設定コマンド</param>
        public static MenuItem CreateExternalAppItem(bool isEnabled, ICommand command, ICommand OpenExternalAppDialogCommand)
        {
            var subItem = new MenuItem() { Header = Properties.TextResources.GetString("BookshelfItem.Menu.OpenExternalApp"), IsEnabled = isEnabled };

            if (Config.Current.System.ExternalAppCollection.Any())
            {
                for (int i = 0; i < Config.Current.System.ExternalAppCollection.Count; ++i)
                {
                    var folder = Config.Current.System.ExternalAppCollection[i];
                    var header = new TextBlock(new Run(folder.DispName));
                    subItem.Items.Add(new MenuItem() { Header = header, ToolTip = folder.Command, Command = command, CommandParameter = folder });
                }
            }
            else
            {
                subItem.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("Word.ItemNone"), IsEnabled = false });
            }

            subItem.Items.Add(new Separator());
            subItem.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("BookshelfItem.Menu.ExternalAppOption"), Command = OpenExternalAppDialogCommand });

            return subItem;
        }


    }


}
