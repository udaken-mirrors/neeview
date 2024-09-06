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

        public void OpenExternalAppMenu()
        {
            MainViewExternalAppTools.UpdateExternalAppMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void OpenCopyToFolderMenu(ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            MainViewCopyToFolderTools.UpdateCopyToFolderMenu(_contextMenu.Items, parameterFactory);
            _contextMenu.IsOpen = true;
        }

        public void Close()
        {
            _contextMenu.IsOpen = false;
        }

    }
}
