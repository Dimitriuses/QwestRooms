using System.Collections.Generic;

namespace QwestRooms.BLL.Filtering
{
    /// <summary>
    /// One page of results plus the total number of rows that matched, so the caller can render
    /// a pager without having to count the rows itself.
    /// </summary>
    public class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public IReadOnlyList<T> Items { get; private set; }

        /// <summary>Total rows matching the filter, across all pages.</summary>
        public int TotalCount { get; private set; }

        public int PageNumber { get; private set; }

        public int PageSize { get; private set; }
    }
}
