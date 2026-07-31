namespace QwestRooms.BLL.Filtering;

/// <summary>
/// One page of results plus the total number of rows that matched, so a caller can render a pager
/// without counting the rows itself.
/// </summary>
/// <remarks>
/// The pager arithmetic lives here rather than in a web view model because it is a property of the
/// result, not of the page rendering it: the same numbers are what a JSON API would return.
/// </remarks>
public sealed record PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>The rows on this page. Empty for a page past the end of the results.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Rows matching the filter across every page, not just this one.</summary>
    public int TotalCount { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    /// <summary>Pages the matches occupy, rounded up. Zero when nothing matched.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
