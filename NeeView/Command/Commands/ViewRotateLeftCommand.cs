﻿using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
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



    /// <summary>
    /// ビュー回転コマンド用パラメータ
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter
    {
        private int _angle;

        // 属性に説明文
        [PropertyRange("@ParamCommandParameterRotateAmount", 0, 180, Tips = "@ParamCommandParameterRotateAmountTips")]
        public int Angle
        {
            get { return _angle; }
            set { _angle = MathUtility.Clamp(value, 0, 180); }
        }

        // 属性に説明文
        [PropertyMember("@ParamCommandParameterRotateIsStretch", Tips = "@ParamCommandParameterRotateIsStretchTips")]
        public bool IsStretch { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewRotateCommandParameter;
            if (target == null) return false;
            return this == target || (this.Angle == target.Angle && this.IsStretch == target.IsStretch);
        }
    }
}
