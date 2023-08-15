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
            var pageRange = new PageRange(pagePosition, 1);
            Assert.Equal(pagePosition, pageRange.Min);
            Assert.Equal(pagePosition, pageRange.Max);
            Assert.Equal(1, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, 2);
            Assert.Equal(pagePosition, pageRange.Min);
            Assert.Equal(new PagePosition(1, 1), pageRange.Max);
            Assert.Equal(2, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, 3);
            Assert.Equal(pagePosition, pageRange.Min);
            Assert.Equal(new PagePosition(2, 0), pageRange.Max);
            Assert.Equal(3, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, -1);
            Assert.Equal(pagePosition, pageRange.Min);
            Assert.Equal(pagePosition, pageRange.Max);
            Assert.Equal(1, pageRange.PartSize);

            pageRange = new PageRange(pagePosition, -2);
            Assert.Equal(new PagePosition(0, 1), pageRange.Min);
            Assert.Equal(pagePosition, pageRange.Max);
            Assert.Equal(2, pageRange.PartSize);

            var pagePositionHalf = new PagePosition(1, 1);
            pageRange = new PageRange(pagePositionHalf, 1);
            Assert.Equal(pagePositionHalf, pageRange.Min);
            Assert.Equal(pagePositionHalf, pageRange.Max);
            Assert.Equal(1, pageRange.PartSize);

            pageRange = new PageRange(pagePositionHalf, 2);
            Assert.Equal(pagePositionHalf, pageRange.Min);
            Assert.Equal(new PagePosition(2, 0), pageRange.Max);
            Assert.Equal(2, pageRange.PartSize);
        }
    }
}
