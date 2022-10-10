using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class PixelateEffectUnit : EffectUnit
    {
        private static readonly PixelateEffect _effect = new();
        public override Effect GetEffect() => _effect;


        [PropertyRange(0, 1)]
        [DefaultValue(0.75)]
        public double Pixelation
        {
            get { return _effect.Pixelation; }
            set { if (_effect.Pixelation != value) { _effect.Pixelation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
