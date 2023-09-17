using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    public class MenuNode
    {
        public MenuNode(string? name, MenuElementType menuElementType, string? commandName)
        {
            Name = name;
            MenuElementType = menuElementType;
            CommandName = commandName;
        }

        public string? Name { get; set; }

        public MenuElementType MenuElementType { get; set; }

        public string? CommandName { get; set; }

        public List<MenuNode>? Children { get; set; }


        public IEnumerable<MenuNode> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var subChild in child.GetEnumerator())
                    {
                        yield return subChild;
                    }
                }
            }
        }
    }


    public class ContextMenuManager : ContextMenuSetting
    {
        static ContextMenuManager() => Current = new ContextMenuManager();
        public static ContextMenuManager Current { get; }


        public MenuNode? CreateContextMenuNode()
        {
            return SourceTreeRaw?.CreateMenuNode();
        }

        public List<object> CreateContextMenuItems()
        {
            return SourceTree.CreateContextMenuItems();
        }

        internal void Restore(MenuNode? contextMenuNode)
        {
            var sourceTree = contextMenuNode != null ? MenuTree.CreateMenuTree(contextMenuNode) : null;
            sourceTree?.Validate();
            SourceTree = sourceTree;
        }
    }


    // TODO: バージョンのコマンド名変更対応
    public class ContextMenuSetting : BindableBase
    {
        private ContextMenu? _contextMenu;
        private bool _isDirty = true;
        private MenuTree? _sourceTree;


        public ContextMenuSetting()
        {
        }

        public ContextMenuSetting(MenuTree? source)
        {
            _sourceTree = source;
        }


        public ContextMenu? ContextMenu
        {
            get
            {
                _contextMenu = this.IsDirty ? SourceTree.CreateContextMenu() : _contextMenu;
                _isDirty = false;
                return _contextMenu;
            }
        }

        public MenuTree? SourceTreeRaw
        {
            get => _sourceTree;
            set => _sourceTree = value;
        }

        [NotNull]
        public MenuTree? SourceTree
        {
            get { return _sourceTree ?? MenuTree.CreateDefault(); }
            set
            {
                _sourceTree = value;
                _contextMenu = null;
                _isDirty = true;
                RaisePropertyChanged();
            }
        }

        public bool IsDirty
        {
            get { return _isDirty || _contextMenu == null; }
            set { _isDirty = value; }
        }

        public ContextMenuSetting Clone()
        {
            return new ContextMenuSetting(_sourceTree?.Clone());
        }

        public void Validate()
        {
            _sourceTree?.Validate();
        }
    }
}
