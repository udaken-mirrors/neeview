using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    public class CopyFileCommandParameter : CommandParameter
    {
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // 複数ページのときの動作
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }


        #region Obsolete

        // 圧縮ファイルのときの動作
        [Obsolete("no used"), Alternative("nv.Config.ArchiveCopyPolicy", 42)] // ver.42
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ArchivePolicy ArchivePolicy
        {
            get => default;
            set => ArchivePolicy_Legacy = value;
        }

        [Obsolete, JsonIgnore]
        public ArchivePolicy ArchivePolicy_Legacy { get; private set; }

        // テキストコピーのヒント
        [Obsolete("no used"), Alternative("nv.Config.TextCopyPolicy", 42)] // ver.42
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TextCopyPolicy TextCopyPolicy
        {
            get => default;
            set => TextCopyPolicy_Legacy = value;
        }

        [Obsolete, JsonIgnore]
        public TextCopyPolicy TextCopyPolicy_Legacy { get; private set; }

        #endregion Obsolete
    }
}
