namespace QwestRoom.BLL.Filtering
{
    /// <summary>
    /// The criteria a room list can be narrowed by. All properties are optional; a filter with
    /// every property null matches everything.
    /// <para>
    /// This replaces the previous arrangement, where the selected country/city/address were held
    /// in <c>Session</c> and read back inside the controller. Passing them explicitly means a
    /// filtered list can be linked to, bookmarked, opened in two tabs at once, and combined with
    /// paging.
    /// </para>
    /// </summary>
    public class RoomFilter
    {
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? AddressId { get; set; }

        public static RoomFilter None
        {
            get { return new RoomFilter(); }
        }
    }
}
