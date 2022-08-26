// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

namespace NeeView
{
    /// <summary>
    /// 廃棄されたコマンドの情報
    /// </summary>
    public class ObsoleteCommandItem
    {
        public ObsoleteCommandItem(string obsolete, string? alternative, int version)
        {
            Obsolete = obsolete;
            Alternative = alternative;
            Version = version;
        }

        public string Obsolete { get; }
        public string? Alternative { get; }
        public int Version { get; }
    }
}
