﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{

    /// <summary>
    /// MessageBoxEx.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageBoxEx : Window
    {
        MessageBoxParams _Param;


        public MessageBoxEx(MessageBoxParams param)
        {
            _Param = param;

            InitializeComponent();

            this.Title = param.Caption;
            this.MessageBoxText.Text = param.MessageBoxText;

            switch (param.Button)
            {
                case MessageBoxButton.OK:
                    this.YesButton.Content = "OK";
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    this.YesButton.Content = "OK";
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Content = "Cancel";
                    break;
                case MessageBoxButton.YesNo:
                    this.YesButton.Content = "Yes";
                    this.NoButton.Content = "No";
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.YesButton.Content = "Yes";
                    this.NoButton.Content = "No";
                    this.CancelButton.Content = "Cancel";
                    break;
                default:
                    throw new NotSupportedException();
            }

            this.YesButton.Focus(); // Yesボタンにフォーカス

            // メッセージボックスのアイコン
            switch (param.Icon)
            {
                case MessageBoxExImage.Warning:
                    this.IconImage.Source = App.Current.Resources["ic_warning_48px"] as ImageSource;
                    System.Media.SystemSounds.Exclamation.Play();
                    break;

                case MessageBoxExImage.Error:
                    this.IconImage.Source = App.Current.Resources["ic_error_48px"] as ImageSource;
                    System.Media.SystemSounds.Exclamation.Play();
                    break;

                case MessageBoxExImage.RecycleBin:
                    this.IconImage.Source = App.Current.Resources["ic_delete_48px"] as ImageSource;
                    break;

                case MessageBoxExImage.Information:
                    this.IconImage.Source = App.Current.Resources["ic_warning_48px"] as ImageSource;
                    break;

                case MessageBoxExImage.Question:
                    this.IconImage.Source = App.Current.Resources["ic_help_24px"] as ImageSource;
                    break;

                default:
                    this.IconImage.Visibility = Visibility.Collapsed;
                    break;
            }

            // Visual
            if (param.VisualContent != null)
            {
                this.VisualControl.Content = param.VisualContent;
                this.VisualControl.Margin = new Thickness(0, 0, 20, 0);
            }

        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
