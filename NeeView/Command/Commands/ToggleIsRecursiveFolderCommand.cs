using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsRecursiveFolderCommand : CommandElement
    {
        public ToggleIsRecursiveFolderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsRecursiveFolder));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsRecursiveFolder ? Properties.Resources.ToggleIsRecursiveFolderCommand_Off : Properties.Resources.ToggleIsRecursiveFolderCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsRecursiveFolder(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettings.Current.ToggleIsRecursiveFolder();
            }
        }
    }
}
