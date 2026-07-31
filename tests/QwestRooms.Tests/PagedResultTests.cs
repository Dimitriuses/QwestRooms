using QwestRooms.BLL.Filtering;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// Pager arithmetic. This is not busywork: the page-number loop in the 2019 view shipped as
/// <c>i &lt; TotalPages</c>, so the last page was unreachable and nothing caught it. The boundary
/// cases below are exactly the ones that were wrong.
/// </summary>
public sealed class PagedResultTests
{
    [Theory]
    [InlineData(450, 27, 17)] // the demo dataset: 16.67 pages, so 17
    [InlineData(54, 27, 2)]   // an exact multiple
    [InlineData(28, 27, 2)]   // one item into the second page
    [InlineData(27, 27, 1)]   // exactly one full page
    [InlineData(1, 27, 1)]    // a single item still needs a page
    [InlineData(0, 27, 0)]    // nothing matched: no pages at all
    public void TotalPages_RoundsUp(int totalCount, int pageSize, int expected)
    {
        var page = Page(totalCount, 1, pageSize);

        Assert.Equal(expected, page.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_IsFalse_OnTheFirstPage() => Assert.False(Page(450, 1, 27).HasPreviousPage);

    [Fact]
    public void HasPreviousPage_IsTrue_BeyondTheFirstPage() => Assert.True(Page(450, 2, 27).HasPreviousPage);

    [Fact]
    public void HasNextPage_IsTrue_OnThePenultimatePage() => Assert.True(Page(450, 16, 27).HasNextPage);

    [Fact]
    public void HasNextPage_IsFalse_OnTheLastPage() => Assert.False(Page(450, 17, 27).HasNextPage);

    [Fact]
    public void NothingMatched_HasNeitherANextNorAPreviousPage()
    {
        var page = Page(0, 1, 27);

        Assert.False(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Rejects_NonPositivePageSize(int pageSize) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Page(100, 1, pageSize));

    [Fact]
    public void Constructor_Rejects_NegativeTotalCount() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Page(-1, 1, 27));

    private static PagedResult<string> Page(int totalCount, int pageNumber, int pageSize) =>
        new([], totalCount, pageNumber, pageSize);
}
