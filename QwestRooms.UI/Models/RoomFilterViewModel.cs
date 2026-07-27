namespace QwestRooms.UI.Models
{
    /// <summary>
    /// The currently applied filter, bound from the query string. Held on the view model so the
    /// pager links can carry the criteria forward -- that is what lets filtering and paging be
    /// combined, which the old Session-based version could not do.
    /// </summary>
    public class RoomFilterViewModel
    {
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? AddressId { get; set; }
    }
}
