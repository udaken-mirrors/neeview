﻿using System;

namespace NeeView
{
    public record class HistoryItemAccessor
    {
        private readonly BookHistory _source;

        public HistoryItemAccessor(BookHistory source)
        {
            _source = source;
        }

        internal BookHistory Source => _source;

        [WordNodeMember]
        public string Name => _source.Name;

        [WordNodeMember]
        public string Path => _source.Path;

        [WordNodeMember]
        public string LastAccessTime => _source.LastAccessTime.ToString();
    }
}
