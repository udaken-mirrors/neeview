using System;
using System.Windows.Controls;

namespace NeeView
{
    public class CommandMenuAdapter
    {
        private readonly ContextMenu _contextMenu;

        public CommandMenuAdapter(ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void Open(CommandMenuType menuType)
        {
            if (_contextMenu.IsOpen)
            {
                _contextMenu.IsOpen = false;
                return;
            }

            Update(menuType);

            _contextMenu.IsOpen = true;
        }

        public void Close()
        {
            _contextMenu.IsOpen = false;
        }

        private void Update(CommandMenuType menuType)
        {
            switch (menuType)
            {
                case CommandMenuType.ExternalApp:
                    MainViewExternalAppTools.UpdateExternalAppMenu(_contextMenu.Items);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }


    public enum CommandMenuType
    {
        /// <summary>
        /// 外部アプリ メニュー
        /// </summary>
        ExternalApp,

        /// <summary>
        /// フォルダーへコピー メニュー
        /// </summary>
        CopyToFolder,

        /// <summary>
        /// フォルダーへ移動 メニュー
        /// </summary>
        MoveToFolder,
    }
}
