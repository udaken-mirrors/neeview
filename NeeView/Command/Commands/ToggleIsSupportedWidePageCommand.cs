﻿using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedWidePageCommand : CommandElement
    {
        public ToggleIsSupportedWidePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedWidePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedWidePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage));
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage ? Properties.Resources.CommandToggleIsSupportedWidePageOff : Properties.Resources.CommandToggleIsSupportedWidePageOn;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookSettingPresenter.Current.ToggleIsSupportedWidePage();
        }
    }
}
