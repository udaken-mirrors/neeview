﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class DelayVisibility : BindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Changed;


        /// <summary>
        /// MenuLayerVisibility property.
        /// </summary>
        public Visibility Visibility
        {
            get { return Get(); }
            set { Set(value); }
        }

        //
        private DelayValue<Visibility> _visibility;

        //
        public double DefaultDelayTime { get; set; } = 1.0;

        //
        public DelayVisibility()
        {
            _visibility = new DelayValue<Visibility>(Visibility.Collapsed);
            _visibility.ValueChanged += (s, e) =>
            {
                Changed?.Invoke(s, e);
                RaisePropertyChanged(nameof(Visibility));
            };
        }

        //
        public Visibility Get()
        {
            return _visibility.Value;
        }

        //
        public void Set(Visibility visibility)
        {
            var delay = this.DefaultDelayTime * 1000;
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : delay);
        }

        //
        public void SetDelayVisibility(Visibility visibility, int ms)
        {
            _visibility.SetValue(visibility, ms);
        }

        //
        public string ToDetail()
        {
            return _visibility.ToDetail();
        }
    }
}
