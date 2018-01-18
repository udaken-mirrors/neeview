﻿// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;
using System.Reflection;

namespace NeeView.Effects
{
    public class LevelEffect : ShaderEffect
    {
        private static PixelShader s_pixelShader = new PixelShader()
        {
            UriSource = Tools.MakePackUri(typeof(LevelEffect).Assembly, "Effects/Shaders/LevelEffect.ps")
        };

        public LevelEffect()
        {
            PixelShader = s_pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BlackProperty);
            UpdateShaderValue(WhiteProperty);
            UpdateShaderValue(CenterProperty);
            UpdateShaderValue(MinimumProperty);
            UpdateShaderValue(MaximumProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(LevelEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        //
        public static readonly DependencyProperty BlackProperty = DependencyProperty.Register("Black", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Black
        {
            get { return (double)GetValue(BlackProperty); }
            set { SetValue(BlackProperty, value); }
        }

        //
        public static readonly DependencyProperty WhiteProperty = DependencyProperty.Register("White", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(1)));
        public double White
        {
            get { return (double)GetValue(WhiteProperty); }
            set { SetValue(WhiteProperty, value); }
        }

        //
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        //
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(3)));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        //
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(4)));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
    }
}
