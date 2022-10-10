using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }
        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollUp(e.Parameter.Cast<ViewScrollCommandParameter>());
        }
    }


    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    public class ViewScrollCommandParameter : CommandParameter
    {
        private double _scroll = 0.25;
        private bool _allowCrossScroll = true;
        private double _scrollDuration = 0.1;

        // 属性に説明文
        [PropertyPercent]
        public double Scroll
        {
            get { return _scroll; }
            set { SetProperty(ref _scroll, MathUtility.Clamp(value, 0.0, 1.0)); }
        }

        // スクロール速度(秒)
        [PropertyMember]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyMember]
        public bool AllowCrossScroll
        {
            get => _allowCrossScroll;
            set => SetProperty(ref _allowCrossScroll, value);
        }
    }

}
