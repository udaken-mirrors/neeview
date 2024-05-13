using System;

namespace NeeView
{
    public static class FolderNodeAccessorFactory
    {
        public static FolderNodeAccessor Create(FolderTreeModel model, FolderTreeNodeBase node)
        {
            return node switch
            {
                RootQuickAccessNode n
                    => new RootQuickAccessNodeAccessor(model, n),
                QuickAccessNode n
                    => new QuickAccessNodeAccessor(model, n),
                RootDirectoryNode n
                    => new RootDirectoryNodeAccessor(model, n),
                DirectoryNode n
                    => new DirectoryNodeAccessor(model, n),
                RootBookmarkFolderNode n
                    => new RootBookmarkNodeAccessor(model, n),
                BookmarkFolderNode n
                    => new BookmarkNodeAccessor(model, n),
                DummyNode n
                    => new FolderNodeAccessor(model, n),
                _
                    => throw new NotSupportedException($"Not support yet: {node.GetType().FullName}"),
            };
        }
    }
}
