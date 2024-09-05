using System;
using System.Windows.Controls;

namespace NeeView
{
    public class OpenExternalAppAsCommand : CommandElement
    {
        public OpenExternalAppAsCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppAsCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var index = e.Parameter.Cast<OpenExternalAppAsCommandParameter>().Index - 1;
            if (index >= 0)
            {
                return MainViewExternalAppTools.CanOpenExternalApp(index);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var index = e.Parameter.Cast<OpenExternalAppAsCommandParameter>().Index - 1;
            if (index >= 0)
            {
                MainViewExternalAppTools.OpenExternalApp(index);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.Open(CommandMenuType.ExternalApp);
            }
        }

        public override MenuItem? CreateMenuItem()
        {
            return MainViewExternalAppTools.CreateExternalAppItem();
        }
    }

}
