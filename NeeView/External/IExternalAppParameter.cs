namespace NeeView
{
    public interface IExternalAppParameter
    {
        /// <summary>
        /// コマンド
        /// </summary>
        string? Command { get; set; }

        /// <summary>
        /// コマンドパラメーター
        /// $File = 渡されるファイルパス
        /// </summary>
        string Parameter { get; set; }

        /// <summary>
        /// 作業フォルダー
        /// </summary>
        string? WorkingDirectory { get; set; }

        /// <summary>
        /// 複数ページのときの動作
        /// メインビューを処理するときに使用される
        /// </summary>
        MultiPagePolicy MultiPagePolicy { get; set; }

        /// <summary>
        /// 圧縮ファイルのときの動作
        /// </summary>
        ArchivePolicy ArchivePolicy { get; set; }
    }

}
