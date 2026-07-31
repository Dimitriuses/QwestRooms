namespace QwestRooms.BLL.Dtos;

public sealed record CountryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
