﻿namespace NeeView
{
    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteFile;
            this.MenuText = Properties.Resources.CommandDeleteFileMenu;
            this.Note = Properties.Resources.CommandDeleteFileNote;
            this.ShortCutKey = "Delete";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanDeleteFile();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            var async = BookOperation.Current.DeleteFileAsync();
        }
    }
}
