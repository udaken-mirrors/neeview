﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// クイックアクセスのフォルダーコレクション
    /// (未使用)
    /// </summary>
    public class QuickAccessFolderCollection : FolderCollection, IDisposable
    {
        public QuickAccessFolderCollection() : base(new QueryPath(QueryScheme.QuickAccess, null), false)
        {
            var items = QuickAccessCollection.Current.Items.Select(e => CreateFolderItem(e));

            this.Items = new ObservableCollection<FolderItem>(items);

            //TODO:
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }

        private void QuickAccessCollection_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            var target = e.Element as QuickAccess;

            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    {
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(target);
                            var index = QuickAccessCollection.Current.Items.IndexOf(target);
                            InsertItem(item, index);
                        }
                    }
                    break;

                case CollectionChangeAction.Remove:
                    {
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;


                case CollectionChangeAction.Refresh:
                    FolderList.Current.RequestPlace(new QueryPath(QueryScheme.QuickAccess, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                    break;
            }
        }



        private FolderItem CreateFolderItem(QuickAccess quickAccess)
        {
            return new ConstFolderItem(new FolderThumbnail())
            {
                Source = quickAccess,
                Type = FolderItemType.Directory,
                Place = Place,
                Name = quickAccess.Name,
                TargetPath = new QueryPath(quickAccess.Path),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.System | FolderItemAttribute.QuickAccess,
                IsReady = true
            };
        }

        private void InsertItem(FolderItem item, int index)
        {
            if (item == null) return;

            if (this.Items.Count == 1 && this.Items.First().Type == FolderItemType.Empty)
            {
                this.Items.RemoveAt(0);
                this.Items.Add(item);
            }
            else if (index >= 0)
            {
                this.Items.Insert(index, item);
            }
            else
            {
                this.Items.Add(item);
            }
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    QuickAccessCollection.Current.CollectionChanged -= QuickAccessCollection_CollectionChanged;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }

}