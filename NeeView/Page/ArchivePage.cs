﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// フォルダサムネイル専用ページ.
    /// Pageの仕組みを使用してサムネイルを作成する
    /// </summary>
    public class ArchivePage : Page
    {
        // コンストラクタ
        public ArchivePage(string place, string entryName = null)
        {
            Entry = new ArchiveEntry() { EntryName = place };
            Content = new ArchiveContent(Entry, entryName);
        }
    }

}
