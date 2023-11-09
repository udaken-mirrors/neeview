using NeeLaboratory.IO.Nodes;
using System.Diagnostics;

namespace NeeLaboratory.IO.NodesTest
{
    public class NodeTest
    {
        [Fact]
        public void NodeBasicTest()
        {

            var node1a = new Node("1A");
            Assert.Null(node1a.Children);
            Assert.Null(node1a.Parent);

            // add 2a
            var node2a = node1a.AddChild("2A");
            Assert.Null(node1a.Parent);
            Assert.NotNull(node1a.Children);
            Assert.Equal(1, node1a.Children?.Count);
            Assert.Equal(node1a, node2a.Parent);
            Assert.Null(node2a.Children);

            // add 2b
            var node2b = node1a.AddChild("2B");
            Assert.Null(node1a.Parent);
            Assert.NotNull(node1a.Children);
            Assert.Equal(2, node1a.Children?.Count);
            Assert.Equal(node1a, node2b.Parent);
            Assert.Null(node2b.Children);

            // remove 2a
            var removed = node1a.RemoveChild("2A");
            Assert.Equal(node2a, removed);
            Assert.Null(node2a.Parent);
            Assert.Equal(1, node1a.Children?.Count);

            // remove 2b
            removed = node1a.RemoveChild("2B");
            Assert.Equal(node2b, removed);
            Assert.Null(node2b.Parent);
            Assert.Equal(0, node1a.Children?.Count);
        }

        [Fact]
        public void NodeAddTest()
        {
            Assert.Throws<ArgumentException>(() => new Node("A\\B"));
            Assert.Equal("C:", new Node("C:\\").Name);

            var node1a = new Node("1A");
            var node2a = node1a.AddChild("2A");
            var node2b = node1a.AddChild("2B");

            // add empty exception
            Assert.Throws<ArgumentException>(() => node1a.AddChild(""));

            // add already exception
            Assert.Throws<ArgumentException>(() => node1a.AddChild("2A"));
        }

        [Fact]
        public void NodeRemoveTest()
        {
            var node1a = new Node("1A");
            var node2a = node1a.AddChild("2A");
            var node2b = node1a.AddChild("2B");

            // remove not child node
            Assert.Null(node1a.RemoveChild("2C"));
            Assert.Equal(2, node1a.Children?.Count);

            // remove not child node
            var node2ad = new Node("2A");
            Assert.Null(node1a.RemoveChild(node2ad));
            Assert.Equal(2, node1a.Children?.Count);
        }

        [Fact]
        public void NodeWalkTest()
        {
            var node1a = new Node("1A");
            var node2a = node1a.AddChild("2A");
            var node2b = node1a.AddChild("2B");
            var node3ax = node2a.AddChild("3AX");
            var node3ay = node2a.AddChild("3AY");

            Assert.Equal(new[] { "1A", "2A", "3AX", "3AY", "2B" }, node1a.Walk().Select(e => e.Name));
            Assert.Equal(new[] { "2A", "3AX", "3AY", "2B" }, node1a.WalkChildren().Select(e => e.Name));

            Assert.Equal(@"1A", node1a.FullName);
            Assert.Equal(@"1A\2A", node2a.FullName);
            Assert.Equal(@"1A\2B", node2b.FullName);
            Assert.Equal(@"1A\2A\3AX", node3ax.FullName);
            Assert.Equal(@"1A\2A\3AY", node3ay.FullName);

            Assert.True(node2a.HasParent(node1a));
            Assert.True(node3ay.HasParent(node1a));
            Assert.True(node3ay.HasParent(node2a));
            Assert.False(node3ay.HasParent(node2b));
        }
    }
}