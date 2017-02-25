﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace NeeView
{
    /// <summary>
    /// 履歴
    /// </summary>
    public class BookHistory : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // 履歴変更イベント
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // 履歴コレクション
        // 膨大な数で変更が頻繁に行われるのでLinkedList
        public LinkedList<BookMementoUnit> Items { get; set; }

        // フォルダリストで開いていた場所
        public string LastFolder { get; set; }

        // フォルダとソートの種類
        private Dictionary<string, FolderOrder> _folderOrders;

        // 履歴制限
        private int _limitSize;

        // 履歴制限(時間)
        private TimeSpan _limitSpan;

        /// <summary>
        /// IsKeepFolderStatus property.
        /// </summary>
        public bool IsKeepFolderStatus { get; set; } = true;


        // フォルダ設定
        public void SetFolderOrder(string path, FolderOrder order)
        {
            path = path ?? "<<root>>";

            // 名前順は記憶しない。それ以外の場合記憶する
            if (order == FolderOrder.FileName)
            {
                _folderOrders.Remove(path);
            }
            else
            {
                _folderOrders[path] = order;
            }
        }

        // フォルダ設定取得
        public FolderOrder GetFolderOrder(string path)
        {
            path = path ?? "<<root>>";

            FolderOrder order;
            _folderOrders.TryGetValue(path, out order);
            return order;
        }


        /// <summary>
        /// 
        /// </summary>
        public BookHistory()
        {
            Items = new LinkedList<BookMementoUnit>();
            _folderOrders = new Dictionary<string, FolderOrder>();
        }

        // 要素数
        public int Count => Items.Count;

        // 先頭の要素
        public BookMementoUnit First => Items.First();

        // 履歴クリア
        public void Clear()
        {
            foreach (var node in Items)
            {
                node.HistoryNode = null;
            }
            Items.Clear();

            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Clear, null));
        }


        //
        public void Load(IEnumerable<Book.Memento> items)
        {
            // 履歴リストをクリア
            Clear();

            //
            foreach (var item in items)
            {
                var unit = ModelContext.BookMementoCollection.Find(item.Place);

                if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);

                    ModelContext.BookMementoCollection.Add(unit);
                }
                else
                {
                    unit.Memento = item;
                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddLast(unit.HistoryNode);
                }
            }

            LimitNow();
            HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Load, null));
        }

        // 履歴更新
        public BookMementoUnit Add(BookMementoUnit unit, Book.Memento memento, bool isKeepOrder)
        {
            if (memento == null) return unit;
            Debug.Assert(unit == null || unit.Memento.Place == memento.Place);

            try
            {
                if (isKeepOrder)
                {
                    if (unit?.HistoryNode != null)
                    {
                        var lastAccessTime = unit.Memento?.LastAccessTime ?? DateTime.Now;
                        unit.Memento = memento;
                        unit.Memento.LastAccessTime = lastAccessTime;

                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                }
                else if (unit == null)
                {
                    unit = new BookMementoUnit();

                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);
                    LimitNow();

                    ModelContext.BookMementoCollection.Add(unit);
                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }
                else if (unit.HistoryNode != null)
                {
                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    if (Items.First == unit.HistoryNode)
                    {
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Update, memento.Place));
                    }
                    else
                    {
                        Items.Remove(unit.HistoryNode);
                        Items.AddFirst(unit.HistoryNode);
                        HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                    }
                }
                else
                {
                    unit.Memento = memento;
                    unit.Memento.LastAccessTime = DateTime.Now;

                    unit.HistoryNode = new LinkedListNode<BookMementoUnit>(unit);
                    Items.AddFirst(unit.HistoryNode);
                    LimitNow();

                    HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Add, memento.Place));
                }

                return unit;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        // 履歴追加
        public void Add(Book book, bool isKeepOrder)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var memento = book.CreateMemento();
            var unit = ModelContext.BookMementoCollection.Find(memento.Place);

            Add(unit, memento, isKeepOrder);
        }


        // 履歴削除
        public void Remove(string place)
        {
            var unit = ModelContext.BookMementoCollection.Find(place);
            if (unit != null && unit.HistoryNode != null)
            {
                Items.Remove(unit.HistoryNode);
                unit.HistoryNode = null;
                HistoryChanged?.Invoke(this, new BookMementoCollectionChangedArgs(BookMementoCollectionChangedType.Remove, unit.Memento.Place));
            }
        }

        // 履歴数制限 現在のリスト
        private bool LimitNow()
        {
            return false;

            // フォルダリストに不具合が出るので処理無効
#if false
            int oldCount = Items.Count;

            // limit size
            if (LimitSize != 0)
            {
                while (Items.Count > LimitSize)
                {
                    var last = Items.Last();
                    Items.Remove(last);
                    last.HistoryNode = null;
                }
            }

            // limit time
            if (LimitSpan != default(TimeSpan))
            {
                var limitTime = DateTime.Now - LimitSpan;
                while (Items.Last.Value.Memento.LastAccessTime < limitTime)
                {
                    var last = Items.Last();
                    Items.Remove(last);
                    last.HistoryNode = null;
                }
            }

            return oldCount != Items.Count;
#endif
        }



        // 履歴検索
        public BookMementoUnit Find(string place)
        {
            if (place == null) return null;
            var unit = ModelContext.BookMementoCollection.Find(place);
            return unit?.HistoryNode != null ? unit : null;
        }
        
        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            return Items.Take(size).Select(e => e.Memento).ToList();
        }

        /// <summary>
        /// 範囲指定して履歴をリストアップ
        /// </summary>
        /// <param name="current">基準位置</param>
        /// <param name="direction">方向</param>
        /// <param name="size">取得サイズ</param>
        /// <returns></returns>
        internal List<string> ListUp(string current, int direction, int size)
        {
            var list = new List<string>();
            var now = Find(current);
            var unit = now ?? Items.FirstOrDefault();
            if (now == null && unit != null && direction < 0)
            {
                list.Add(unit.Memento.Place);
            }
            for (int i = 0; i < size; i++)
            {
                unit = direction < 0 ? unit?.HistoryNode?.Next?.Value : unit?.HistoryNode?.Previous?.Value; // リストと履歴の方向は逆
                if (unit == null) break;
                list.Add(unit.Memento.Place);
            }
            return list;
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento : INotifyPropertyChanged
        {
            #region NotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            #endregion

            [DataMember]
            public int _Version { get; set; }

            [DataMember(Name = "History")]
            public List<Book.Memento> Items { get; set; }

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public string LastFolder { get; set; }

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public Dictionary<string, FolderOrder> FolderOrders { get; set; }

            [DataMember(Order = 12)]
            public int LimitSize { get; set; }

            [DataMember(Order = 12)]
            public TimeSpan LimitSpan { get; set; }

            [DataMember(Order = 19)]
            public bool IsKeepFolderStatus { get; set; }

            //
            private void Constructor()
            {
                Items = new List<Book.Memento>();
                FolderOrders = new Dictionary<string, FolderOrder>();
                LimitSize = -1;
                IsKeepFolderStatus = true;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    if (LimitSize == 0) LimitSize = -1;
                }
            }


            // 結合
            public void Merge(Memento memento)
            {
                Items = Items.Concat(memento?.Items).Distinct(new Book.MementoPlaceCompare()).ToList();
            }

            // ファイルに保存
            public void Save(string path)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new System.Text.UTF8Encoding(false);
                settings.Indent = true;
                using (XmlWriter xw = XmlWriter.Create(path, settings))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    serializer.WriteObject(xw, this);
                }
            }

            // ファイルから読み込み
            public static Memento Load(string path)
            {
                using (XmlReader xr = XmlReader.Create(path))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    Memento memento = (Memento)serializer.ReadObject(xr);
                    return memento;
                }
            }
        }

        // memento作成
        public Memento CreateMemento(bool forSave)
        {
            var memento = new Memento();

            memento._Version = App.Config.ProductVersionNumber;
            memento.Items = this.Items.Select(e => e.Memento).ToList();
            memento.FolderOrders = _folderOrders;
            memento.LastFolder = this.LastFolder;
            memento.LimitSize = _limitSize;
            memento.LimitSpan = _limitSpan;
            memento.IsKeepFolderStatus = IsKeepFolderStatus;

            if (forSave)
            {
                // テンポラリフォルダを除外
                memento.Items.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
                // 履歴保持数制限適用
                memento.Items = Limit(memento.Items); // 履歴保持数制限
                // フォルダー保存制限
                if (!memento.IsKeepFolderStatus)
                {
                    memento.FolderOrders = null;
                    memento.LastFolder = null;
                }
            }

            return memento;
        }

        // memento適用
        public void Restore(Memento memento, bool fromLoad)
        {
            this.LastFolder = memento.LastFolder;
            _folderOrders = memento.FolderOrders;
            _limitSize = memento.LimitSize;
            _limitSpan = memento.LimitSpan;
            IsKeepFolderStatus = memento.IsKeepFolderStatus;

            this.Load(fromLoad ? Limit(memento.Items) : memento.Items);
        }


        // 履歴数制限
        private List<Book.Memento> Limit(List<Book.Memento> source)
        {
            // limit size
            var collection = _limitSize == -1 ? source : source.Take(_limitSize);

            // limit time
            var limitTime = DateTime.Now - _limitSpan;
            collection = _limitSpan == default(TimeSpan) ? collection : collection.TakeWhile(e => e.LastAccessTime > limitTime);

            return collection.ToList();
        }


        #endregion
    }
}
