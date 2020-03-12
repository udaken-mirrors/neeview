﻿namespace NeeView
{
    public class NoneCommand : CommandElement
    {
        public NoneCommand() : base("")
        {
            this.Group = "(none)";
            this.Text = "(none)";
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            return;
        }
    }
}
