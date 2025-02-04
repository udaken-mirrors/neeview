﻿using System.Collections.Generic;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    /// QueryPathCollection collection for DataObject
    /// </summary>
    public class QueryPathCollection : List<QueryPath>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), nameof(QueryPathCollection));

        public QueryPathCollection()
        {
        }

        public QueryPathCollection(IEnumerable<QueryPath> collection) : base(collection)
        {
        }
    }

    public static class QueryPathCollectionExtensions
    {
        public static QueryPathCollection ToQueryPathCollection(this IEnumerable<QueryPath> collection)
        {
            return new QueryPathCollection(collection);
        }
    }

}
