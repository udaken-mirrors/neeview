﻿namespace NeeView
{
    public class ViewRotateLeftCommand : CommandElement
    {
        public ViewRotateLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateLeft;
            this.Note = Properties.Resources.CommandViewRotateLeftNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter() { Angle = 45 });
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.ViewRotateLeft((ViewRotateCommandParameter)param);
        }
    }
}