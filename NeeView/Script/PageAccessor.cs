using System.Threading;

namespace NeeView
{
    public record class PageAccessor
    {
        private readonly Page _page;

        public PageAccessor(Page page)
        {
            _page = page;
        }

        internal Page Source => _page;

        [WordNodeMember]
        public int Index => _page.Index;

        [WordNodeMember]
        public string Path => _page.EntryFullName;

        [WordNodeMember]
        public long Size => _page.Length;

        [WordNodeMember]
        public string LastWriteTime => _page.LastWriteTime.ToString();


        [WordNodeMember]
        public string GetMetaValue(string key)
        {
            // TODO: スクリプト実行のキャンセルトークンを指定するように
            return _page.GetMetaValue(key, CancellationToken.None);
        }
    }
}
