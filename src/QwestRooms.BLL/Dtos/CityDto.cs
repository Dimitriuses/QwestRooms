namespace QwestRooms.BLL.Dtos;

public sealed record CityDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
