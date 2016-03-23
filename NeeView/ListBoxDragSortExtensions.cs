﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxのドラッグ&ドロップによる順番入れ替え用ヘルパ
    /// 使用条件：ItemsSource が ObservableCollection<T>
    /// </summary>
    public static class ListBoxDragSortExtension
    {
        // event PreviewDragOver
        // Drop前の受け入れ判定
        public static void PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }


        // event Drop
        public static void Drop<T>(object sender, DragEventArgs e, ObservableCollection<T> items) where T : class
        {
            if (!e.Data.GetDataPresent(typeof(ListBoxItem))) return;

            var listBox = sender as ListBox;

            // ドラッグオブジェクト
            var item = (e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem)?.DataContext as T;
            if (item == null) return;

            // ドラッグオブジェクトが所属しているリスト判定
            if (items.Count > 0 && !items.Contains(item)) return;

            var dropPos = e.GetPosition(listBox);
            int oldIndex = items.IndexOf(item);
            int newIndex = items.Count - 1;
            for (int i = 0; i < items.Count; i++)
            {
                var listBoxItem = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (listBoxItem == null) continue;

                var pos = listBoxItem.TranslatePoint(new Point(0, listBoxItem.ActualHeight / 2), listBox);
                if (dropPos.Y < pos.Y)
                {
                    newIndex = (i > oldIndex) ? i - 1 : i;
                    break;
                }
            }
            if (oldIndex != newIndex)
            {
                items.Move(oldIndex, newIndex);
            }
        }
    }


    /// <summary>
    /// List拡張
    /// </summary>
    public static class ListExtensions
    {
        // List要素の順番変更
        public static void Move<T>(this List<T> list, int a0, int a1)
        {
            if (a0 == a1) return;

            var value = list.ElementAt(a0);

            list.RemoveAt(a0);
            if (a0 < a1) a1--;
            if (a1 > list.Count) a1 = list.Count;
            if (a1 < 0) a1 = 0;

            list.Insert(a1, value);
        }
    }
}
