﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class ViewContentSourceCollection
    {
        #region Constructors

        public ViewContentSourceCollection()
        {
            Range = new PageRange();
            Collection = new List<ViewContentSource>();
        }

        public ViewContentSourceCollection(PageRange range, List<ViewContentSource> collection)
        {
            Range = range;
            Collection = collection;
        }

        #endregion

        #region Properties

        public PageRange Range { get; }
        public List<ViewContentSource> Collection { get; }

        internal bool IsValid => Collection.Count > 0 && Collection.All(e => e.IsValid);

        #endregion
    }
}