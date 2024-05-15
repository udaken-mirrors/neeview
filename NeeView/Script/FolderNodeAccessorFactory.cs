using System;

namespace NeeView
{
    public static class FolderNodeAccessorFactory
    {
        public static NodeAccessor Create(FolderTreeModel model, FolderTreeNodeBase node)
        {
            return node switch
            {
                RootQuickAccessNode n
                    => new RootQuickAccessNodeAccessor(model, n),
                QuickAccessNode n
                    => new QuickAccessNodeAccessor(model, n),
                RootDirectoryNode n
                    => new DirectoryNodeAccessor(model, n),
                DirectoryNode n
                    => new DirectoryNodeAccessor(model, n),
                RootBookmarkFolderNode n
                    => new BookmarkNodeAccessor(model, n),
                BookmarkFolderNode n
                    => new BookmarkNodeAccessor(model, n),
                DummyNode n
                    => new NodeAccessor(model, n),
                _
                    => throw new NotSupportedException($"Not support yet: {node.GetType().FullName}"),
            };
        }
    }
}
