﻿namespace NeeView
{
    public class ViewScrollLeftCommand : CommandElement
    {
        public ViewScrollLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollLeft;
            this.Note = Properties.Resources.CommandViewScrollLeftNote;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            DragTransformControl.Current.ScrollLeft((ViewScrollCommandParameter)param);
        }
    }
}
