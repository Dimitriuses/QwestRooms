using System;

namespace QwestRooms.UI.Models
{
    public class PageViewModel
    {
        public PageViewModel(int totalCount, int pageNumber, int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("pageSize", "Page size must be at least 1.");
            }

            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public int PageNumber { get; private set; }

        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get { return PageNumber > 1; }
        }

        public bool HasNextPage
        {
            get { return PageNumber < TotalPages; }
        }
    }
}
