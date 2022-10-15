using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.UnitTest
{
    public class PagePositionTest
    {
        [Fact]
        public void PageRangeTest()
        {
            var pagePosition = new PagePosition(1, 0);
            var pageRange = new PageRange(pagePosition, 1, 1);
            Assert.Equal(pagePosition, pageRange.Position);
            Assert.Equal(1, pageRange.Direction);
            Assert.Equal(2, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, 1, 2);
            Assert.Equal(pagePosition, pageRange.Position);
            Assert.Equal(1, pageRange.Direction);
            Assert.Equal(4, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, 1, 3);
            Assert.Equal(pagePosition, pageRange.Position);
            Assert.Equal(1, pageRange.Direction);
            Assert.Equal(6, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, -1, 1);
            Assert.Equal(pagePosition, pageRange.Position);
            Assert.Equal(-1, pageRange.Direction);
            Assert.Equal(1, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, -1, 2);
            Assert.Equal(pagePosition, pageRange.Position);
            Assert.Equal(-1, pageRange.Direction);
            Assert.Equal(3, pageRange.PartSize);

            var pagePositionHalf = new PagePosition(1, 1);
            pageRange = new PageRange(pagePositionHalf, 1, 1);
            Assert.Equal(pagePositionHalf, pageRange.Position);
            Assert.Equal(1, pageRange.Direction);
            Assert.Equal(1, pageRange.PartSize);

            pageRange = new PageRange(pagePositionHalf, 1, 2);
            Assert.Equal(pagePositionHalf, pageRange.Position);
            Assert.Equal(1, pageRange.Direction);
            Assert.Equal(3, pageRange.PartSize);
        }
    }
}
