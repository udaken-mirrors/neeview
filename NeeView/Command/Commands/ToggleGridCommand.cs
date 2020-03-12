﻿using System.Windows.Data;


namespace NeeView
{
    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleGrid;
            this.MenuText = Properties.Resources.CommandToggleGridMenu;
            this.Note = Properties.Resources.CommandToggleGridNote;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.GridLine.IsEnabled)) { Mode = BindingMode.OneWay, Source = ContentCanvas.Current.GridLine };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.GridLine.IsEnabled ? Properties.Resources.CommandToggleGridOff : Properties.Resources.CommandToggleGridOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.GridLine.IsEnabled = !ContentCanvas.Current.GridLine.IsEnabled;
        }
    }
}
