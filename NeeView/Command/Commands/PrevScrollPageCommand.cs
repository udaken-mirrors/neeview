using NeeLaboratory.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = "NextScrollPage";

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.ScrollToPrevFrame(sender, e.Parameter.Cast<ScrollPageCommandParameter>());
        }
    }

}
