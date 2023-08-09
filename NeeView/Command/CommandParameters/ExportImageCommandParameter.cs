using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ExportImageCommandParameter : CommandParameter
    {
        private ExportImageMode _mode;
        private bool _hasBackground;
        private string? _exportFolder;
        private ExportImageFileNameMode _fileNameMode;
        private ExportImageFormat _fileFormat;
        private int _qualityLevel = 80;

        [PropertyMember]
        public ExportImageMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        [PropertyMember]
        public bool HasBackground
        {
            get => _hasBackground;
            set => SetProperty(ref _hasBackground, value);
        }

        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
        }

        [PropertyMember]
        public ExportImageFileNameMode FileNameMode
        {
            get => _fileNameMode;
            set => SetProperty(ref _fileNameMode, value);
        }

        [PropertyMember]
        public ExportImageFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value);
        }
    }
}
