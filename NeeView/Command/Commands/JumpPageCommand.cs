using System;

namespace NeeView
{
    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                var number = Convert.ToInt32(e.Args[0]) - 1;
                BookOperation.Current.Control.MoveTo(this, number);
            }
            else
            {
                BookOperation.Current.JumpPageAs(this);
            }
        }
    }
}
