﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// SaveWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SaveWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private Exporter _exporter;

        #region Property: IsHintDoubleImage
        private bool _isHintDoubleImage;
        public bool IsHintDoubleImage
        {
            get { return _isHintDoubleImage; }
            set
            {
                _isHintDoubleImage = value;
                RaisePropertyChanged();
                UpdateExporter();
            }
        }
        #endregion

        public bool IsEnableDoubleImage => !IsHintClone && _exporter.DoubleImage != null;

        #region Property: IsHintBackground
        private bool _isHintBackground;
        public bool IsHintBackground
        {
            get { return _isHintBackground; }
            set
            {
                _isHintBackground = value;
                RaisePropertyChanged();
                UpdateExporter();
            }
        }
        #endregion

        public bool IsEnableBackground => !IsHintClone;

        #region Property: IsHintClone
        private bool _isHintClone;
        public bool IsHintClone
        {
            get { return _isHintClone; }
            set
            {
                _isHintClone = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsEnableDoubleImage));
                RaisePropertyChanged(nameof(IsEnableBackground));
                UpdateExporter();
            }
        }
        #endregion

        #region Property: Thumbnail
        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: ThumbnailTitle
        private string _thumbnailTitle;
        public string ThumbnailTitle
        {
            get { return _thumbnailTitle; }
            set { _thumbnailTitle = value; RaisePropertyChanged(); }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exporter"></param>
        public SaveWindow(Exporter exporter)
        {
            InitializeComponent();

            this.ButtonSave.Focus();

            _exporter = exporter;
            IsHintClone = _exporter.IsHintClone;

            this.DataContext = this;
        }

        /// <summary>
        /// エクスポーター設定を更新
        /// </summary>
        private void UpdateExporter()
        {
            _exporter.IsHintClone = IsHintClone;
            _exporter.ExportType = (!IsHintClone && IsHintDoubleImage) ? ExportType.Double : ExportType.Single;
            _exporter.IsHintBackground = (!IsHintClone && IsHintBackground);

            _exporter.UpdateBitmapSource();

            Thumbnail = _exporter.BitmapSource;
            ThumbnailTitle = _exporter.CurrentImage.Name;
        }

        // 決定ボタン
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = ExporterProfile.Current.IsEnableExportFolder ? ExporterProfile.Current.ExportFolder : null;
            dialog.OverwritePrompt = true;

            dialog.AddExtension = true;

            var defaultExt = _exporter.CurrentImage.DefaultExtension;
            dialog.DefaultExt = defaultExt;

            // 拡張子は小文字限定
            var fileName = LoosePath.ValidFileName(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(_exporter.CurrentImage.Name), defaultExt));
            dialog.FileName = fileName;

            if (!IsHintClone)
            {
                var pngExt = new string[] { ".png" };
                var jpgExt = new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" };

                string filter = "PNG|*.png|JPEG|*.jpg;*.jpeg;*.jpe;*.jfif";

                // クローン保存できない時は標準でPNGにする
                if (!_exporter.CanClone(false))
                {
                    fileName = System.IO.Path.ChangeExtension(fileName, ".png");
                    dialog.FileName = fileName;
                    defaultExt = ".png";
                    dialog.DefaultExt = defaultExt;
                }

                // filter
                if (pngExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 1;
                }
                else if (jpgExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 2;
                }
                else if (_exporter.CanClone(false))
                {
                    filter += $"|{dialog.DefaultExt.ToUpper()}|*.{dialog.DefaultExt}";
                    dialog.FilterIndex = 3;
                }

                dialog.Filter = filter + "|All|*.*";
            }

            if (dialog.ShowDialog(this) == true)
            {
                _exporter.Path = dialog.FileName;
                this.DialogResult = true;
                this.Close();
            }
        }

        // キャンセルボタン
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}