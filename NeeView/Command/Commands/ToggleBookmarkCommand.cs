using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Bookmark;
            this.ShortCutKey = "Ctrl+D";
            this.IsShowMessage = true;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.BookControl.IsBookmark)) { Source = BookOperation.Current.BookControl, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.IsBookmark ? Properties.Resources.ToggleBookmarkCommand_Off : Properties.Resources.ToggleBookmarkCommand_On;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }
        
        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.BookControl.SetBookmark(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookOperation.Current.BookControl.ToggleBookmark();
            }
        }
    }
}
