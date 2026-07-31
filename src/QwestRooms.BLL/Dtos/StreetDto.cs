namespace QwestRooms.BLL.Dtos;

public sealed record StreetDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
