using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    // NOTE: NeeView設定を参照しているので一般化できない
    public static class DataObjectExtensions
    {
        public static void SetQueryDropList(this DataObject data, QueryPath query)
        {
            SetQueryDropList(data, new [] { query });
        }

        public static void SetQueryDropList(this DataObject data, IEnumerable<QueryPath> queries)
        {
            SetQueryDropList(data, queries, Config.Current.System.TextCopyPolicy);
        }

        public static void SetQueryDropList(this DataObject data, IEnumerable<QueryPath> queries, TextCopyPolicy policy)
        {
            if (!queries.Any()) return;

            data.SetData(new QueryPathCollection(queries));

            var files = queries.Where(e => e.Scheme == QueryScheme.File).Select(e => e.SimplePath);
            if (!files.Any()) return;

            var collection = new System.Collections.Specialized.StringCollection();
            foreach(var file in files)
            {
                collection.Add(file);
            }
            data.SetFileDropList(collection);

            // NOTE: ここでは一時ファイルの区別がつかないのでそのままテキスト化する
            if (policy != TextCopyPolicy.None)
            {
                data.SetData(DataFormats.UnicodeText, string.Join(System.Environment.NewLine, files));
            }
        }

    }
}
