using NeeLaboratory.IO.Nodes;

namespace NeeLaboratory.IO.NodesTest
{
    public class NodeTreeTest
    {
        [Fact]
        public void NodeTreeBasicTest()
        {
            var nodeTree = new NodeTree(@"C:\DirA\DirB\");
            Assert.Equal(new[] { "", "C:", "DirA", "DirB" }, nodeTree.Root.Walk().Select(e => e.Name));
            Assert.Equal("DirB", nodeTree.Trunk.Name);
            Assert.Equal(@"C:\DirA\DirB", nodeTree.Trunk.FullName);

            var node1a = nodeTree.Add(@"C:\DirA\DirB\1A");
            Assert.NotNull(node1a);
            Assert.Equal(1, nodeTree.Trunk.Children?.Count);
            Assert.Equal(nodeTree.Trunk, node1a?.Parent);

            var node2a = nodeTree.Add(@"C:\DirA\DirB\1A\2A");
            Assert.NotNull(node2a);
            Assert.Equal(1, node1a?.Children?.Count);
            Assert.Equal(node1a, node2a?.Parent);

            var node2b = nodeTree.Add(@"C:\DirA\DirB\1A\2B");
            Assert.NotNull(node2b);
            Assert.Equal(2, node1a?.Children?.Count);
            Assert.Equal(node1a, node2b?.Parent);

            var node3c = nodeTree.Add(@"C:\DirA\DirB\3A\3B\3C");
            Assert.NotNull(node3c);
            Assert.Equal(2, node1a?.Children?.Count);
            Assert.Equal("3B", node3c?.Parent?.Name);
            Assert.Equal("3A", node3c?.Parent?.Parent?.Name);
            Assert.Equal(@"C:\DirA\DirB\3A\3B\3C", node3c?.FullName);
            Assert.True(node3c?.HasParent(nodeTree.Trunk));

            var node9x = nodeTree.Add(@"C:\DirA\DirX\9X");
            Assert.Null(node9x);
        }

        [Fact]
        public void NodeTreeFindTest()
        {
            var tree = new NodeTree(@"C:\DirA\DirB\");
            var node1a = tree.Add(@"C:\DirA\DirB\1A");
            var node2a = tree.Add(@"C:\DirA\DirB\1A\2A");
            var node2b = tree.Add(@"C:\DirA\DirB\1A\2B");
            var node3c = tree.Add(@"C:\DirA\DirB\3A\3B\3C");

            Assert.Equal(node1a, tree.Find(@"C:\DirA\DirB\1A"));
            Assert.Equal(node2a, tree.Find(@"C:\DirA\DirB\1A\2A"));
            Assert.Equal(node2b, tree.Find(@"C:\DirA\DirB\1A\2B"));
            Assert.Equal(node3c, tree.Find(@"C:\DirA\DirB\3A\3B\3C"));
            Assert.Null(tree.Find(@"C:\DirA\DirB\3A\3B\3X"));
            Assert.Null(tree.Find(@"C:\DirA\DirB\3X\3B\3C"));
            Assert.Null(tree.Find(@"D:\DirA\DirB\3A\3B\3C"));
            Assert.Null(tree.Find(@"C:\DirA\DirB\3X\3B\3C\3D"));
        }

        [Fact]
        public void NodeTreeAddTest()
        {
            var tree = new NodeTree(@"C:\DirA\DirB\");
            var node1a = tree.Add(@"C:\DirA\DirB\1A");
            var node2a = tree.Add(@"C:\DirA\DirB\1A\2A");
            var node2b = tree.Add(@"C:\DirA\DirB\1A\2B");
            var node3c = tree.Add(@"C:\DirA\DirB\3A\3B\3C");

            var node9x = tree.Add(@"C:\DirA\Dir9B\3A\3B\3C");
            Assert.Null(node9x);

            // add empty exception
            Assert.Throws<ArgumentException>(() => tree.Add(""));

            // add same
            var node2ad = tree.Add(@"C:\DirA\DirB\1A\2A");
            Assert.Null(node2ad);
        }

        [Fact]
        public void NodeTreeRemoveTest()
        {
            var tree = new NodeTree(@"C:\DirA\DirB\");
            var node1a = tree.Add(@"C:\DirA\DirB\1A");
            var node2a = tree.Add(@"C:\DirA\DirB\1A\2A");
            var node2b = tree.Add(@"C:\DirA\DirB\1A\2B");
            var node3c = tree.Add(@"C:\DirA\DirB\3A\3B\3C");

            // remove 
            var node2ad = tree.Remove(@"C:\DirA\DirB\1A\2A");
            Assert.Equal(node2a, node2ad);
            Assert.Null(node2ad?.Parent);
            Assert.Equal(1, node1a?.Children?.Count);

            // remove same
            var node2ad2 = tree.Remove(@"C:\DirA\DirB\1A\2A");
            Assert.Null(node2ad2);
        }
    }
}