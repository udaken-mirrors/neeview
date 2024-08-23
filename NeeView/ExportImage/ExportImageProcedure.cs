using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// 画像出力の処理フロー
    /// </summary>
    public static class ExportImageProcedure
    {
        public static void Execute(ExportImageCommandParameter parameter)
        {
            var source = ExportImageSource.Create();

            using var exporter = new ExportImage(source);
            exporter.ExportFolder = string.IsNullOrWhiteSpace(parameter.ExportFolder) ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) : parameter.ExportFolder;
            exporter.Mode = parameter.Mode;
            exporter.HasBackground = parameter.HasBackground;
            exporter.IsOriginalSize = parameter.IsOriginalSize;
            exporter.IsDotKeep = parameter.IsDotKeep;
            exporter.QualityLevel = parameter.QualityLevel;

            string filename = exporter.CreateFileName(parameter.FileNameMode, parameter.FileFormat);
            bool isOverwrite;

            if (string.IsNullOrWhiteSpace(parameter.ExportFolder))
            {
                var dialog = new ExportImageSeveFileDialog(exporter.ExportFolder, filename, exporter.Mode == ExportImageMode.View);
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true) return;
                filename = dialog.FileName;
                isOverwrite = true;
            }
            else
            {
                filename = LoosePath.Combine(exporter.ExportFolder, filename);
                filename = FileIO.CreateUniquePath(filename);
                isOverwrite = false;
            }

            exporter.Export(filename, isOverwrite);

            if (parameter.IsShowToast)
            {
                var toast = new Toast(string.Format(Properties.TextResources.GetString("ExportImage.Message.Success"), filename));
                ToastService.Current.Show(toast);
            }
        }

    }
}
