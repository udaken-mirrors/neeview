﻿using System.Windows.Data;


namespace NeeView
{
    public class SetPageMode2Command : CommandElement
    {
        public SetPageMode2Command(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetPageMode2;
            this.Note = Properties.Resources.CommandSetPageMode2Note;
            this.ShortCutKey = "Ctrl+2";
            this.MouseGesture = "RD";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.WidePage);
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.WidePage);
        }
    }
}
