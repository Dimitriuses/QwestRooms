using QwestRooms.UI.Models;
using System;
using Xunit;

namespace QwestRooms.Tests
{
    /// <summary>
    /// Pager arithmetic. This is not busywork: the page-number loop in the view shipped as
    /// <c>i &lt; TotalPages</c>, so the last page was never reachable, and nothing caught it.
    /// The boundary cases below are exactly the ones that were wrong.
    /// </summary>
    public class PageViewModelTests
    {
        [Theory]
        [InlineData(1000, 27, 38)] // the real dataset: 37.03 pages, so 38
        [InlineData(54, 27, 2)]    // exact multiple
        [InlineData(28, 27, 2)]    // one item into the second page
        [InlineData(27, 27, 1)]    // exactly one full page
        [InlineData(1, 27, 1)]     // a single item still needs a page
        [InlineData(0, 27, 0)]     // nothing matched: no pages at all
        public void TotalPages_RoundsUp(int totalCount, int pageSize, int expected)
        {
            var page = new PageViewModel(totalCount, 1, pageSize);

            Assert.Equal(expected, page.TotalPages);
        }

        [Fact]
        public void HasPreviousPage_IsFalse_OnFirstPage()
        {
            var page = new PageViewModel(1000, 1, 27);

            Assert.False(page.HasPreviousPage);
        }

        [Fact]
        public void HasPreviousPage_IsTrue_BeyondFirstPage()
        {
            var page = new PageViewModel(1000, 2, 27);

            Assert.True(page.HasPreviousPage);
        }

        [Fact]
        public void HasNextPage_IsTrue_OnPenultimatePage()
        {
            var page = new PageViewModel(1000, 37, 27);

            Assert.True(page.HasNextPage);
        }

        [Fact]
        public void HasNextPage_IsFalse_OnLastPage()
        {
            var page = new PageViewModel(1000, 38, 27);

            Assert.False(page.HasNextPage);
        }

        [Fact]
        public void HasNextPage_IsFalse_WhenNothingMatched()
        {
            var page = new PageViewModel(0, 1, 27);

            Assert.False(page.HasNextPage);
            Assert.False(page.HasPreviousPage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_Rejects_NonPositivePageSize(int pageSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PageViewModel(100, 1, pageSize));
        }
    }
}
