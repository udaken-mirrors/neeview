using NeeView.Windows.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeeView
{
    public class DragActionTable : IEnumerable<KeyValuePair<string, DragAction>>
    {
        static DragActionTable() => Current = new DragActionTable();
        public static DragActionTable Current { get; }

        public const string GestureDragActionName = "Gesture";
        private readonly DragActionCollection _defaultMemento;
        private readonly Dictionary<string, DragAction> _elements;


        private DragActionTable()
        {
            var list = new List<DragAction>()
            {
                new AngleDragAction(),
                new AngleSliderDragAction(),
                new ScaleDragAction(),
                new ScaleSliderDragAction(),
                new ScaleSliderCenteredDragAction(),
                new BaseScaleDragAction(),
                new BaseScaleSliderDragAction(),
                new BaseScaleSliderCenteredDragAction(),
                new FlipHorizontalDragAction(),
                new FlipVerticalDragAction(),
                new MoveDragAction(),
                new MoveScaleDragAction(),
                new MarqueeZoomDragAction(),
                new WindowMoveDragAction(),

                new GestureDragAction(GestureDragActionName),

                //new LoupeDragAction(),
                //new HoverDragAction(),
            };

            _elements = list.ToDictionary(e => e.Name);

            _defaultMemento = CreateDragActionCollection();

            Config.Current.Mouse.AddPropertyChanged(nameof(MouseConfig.IsGestureEnabled),
                (s, e) => UpdateGestureDragAction());

            UpdateGestureDragAction();
        }


        public event EventHandler? GestureDragActionChanged;


        public DragAction this[string key]
        {
            get
            {
                if (!_elements.ContainsKey(key)) throw new ArgumentOutOfRangeException(key.ToString());
                return _elements[key];
            }
            set { _elements[key] = value; }
        }

        // コマンドリスト
        [PropertyMember]
        public Dictionary<string, DragAction> Elements => _elements;


        public IEnumerator<KeyValuePair<string, DragAction>> GetEnumerator()
        {
            foreach (var pair in _elements)
            {
                yield return pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        public DragActionCollection CreateDefaultMemento()
        {
            return (DragActionCollection)_defaultMemento.Clone();
        }

        public void UpdateGestureDragAction()
        {
            _elements[GestureDragActionName].DragKey = Config.Current.Mouse.IsGestureEnabled ? new DragKey("RightButton") : new DragKey();
            GestureDragActionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        public string GetActionType(DragKey key)
        {
            if (!key.IsValid) return "";
            return _elements.FirstOrDefault(e => e.Value.DragKey == key).Key;
        }

        /// <summary>
        /// 入力からアクション取得
        /// </summary>
        public DragAction? GetAction(DragKey key)
        {
            return _elements.Values.Where(e => !e.IsDummy).FirstOrDefault(e => e.DragKey == key);
        }

        public bool TryGetValue(DragKey dragKey, [MaybeNullWhen(false)] out DragAction source)
        {
            source = GetAction(dragKey);
            return source is not null;
        }

        public DragActionCollection CreateDragActionCollection()
        {
            var collection = new DragActionCollection();

            foreach (var pair in _elements)
            {
                collection.Add(pair.Key.ToString(), pair.Value.CreateMemento());
            }

            return collection;
        }

        public void RestoreDragActionCollection(DragActionCollection? collection)
        {
            if (collection == null) return;

            foreach (var pair in collection)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
            }
        }
    }

}
