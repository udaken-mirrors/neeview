﻿using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundCustomCommand : CommandElement
    {
        public SetBackgroundCustomCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCustom;
            this.Note = Properties.Resources.CommandSetBackgroundCustomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Custom);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Custom;
        }
    }
}