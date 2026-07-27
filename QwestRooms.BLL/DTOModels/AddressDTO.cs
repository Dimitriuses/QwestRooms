namespace QwestRooms.BLL.DTOModels
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }

        public CountryDTO Country { get; set; }
        public CityDTO City { get; set; }
        public StreetDTO Street { get; set; }
    }
}
