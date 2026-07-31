namespace QwestRooms.BLL.Dtos;

public sealed record AddressDto
{
    public int Id { get; set; }

    public string HouseNumber { get; set; } = string.Empty;

    public CountryDto Country { get; set; } = new();

    public CityDto City { get; set; } = new();

    public StreetDto Street { get; set; } = new();
}
