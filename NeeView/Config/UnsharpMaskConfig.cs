﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// UnsharpMask setting for resize filter (PhotoSauce.MagicScaler)
    /// </summary>
    public class UnsharpMaskConfig : BindableBase, ICloneable
    {
        private int _amount;
        private double _radius;
        private int _threshold;

        /// <summary>
        /// UnsharpAmount property.
        /// 25-200
        /// </summary>
        [PropertyRange("@ParamImageFilterAmount", 25, 200)]
        [DefaultValue(40)]
        public int Amount
        {
            get { return _amount; }
            set { if (_amount != value) { _amount = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// UnsharpRadius property.
        /// 0.3-3.0
        /// </summary>
        [PropertyRange("@ParamImageFilterRadius", 0.3, 3.0)]
        [DefaultValue(1.5)]
        public double Radius
        {
            get { return _radius; }
            set { if (_radius != value) { _radius = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// UnsharpThrethold property.
        /// 0-10
        /// </summary>
        [PropertyRange("@ParamImageFilterThreshold", 0, 10)]
        [DefaultValue(0)]
        public int Threshold
        {
            get { return _threshold; }
            set { if (_threshold != value) { _threshold = value; RaisePropertyChanged(); } }
        }

        public UnsharpMaskSettings CreateUnsharpMaskSetting()
        {
            return new UnsharpMaskSettings(_amount, _radius, (byte)_threshold);
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ Radius.GetHashCode() ^ Threshold.GetHashCode();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}