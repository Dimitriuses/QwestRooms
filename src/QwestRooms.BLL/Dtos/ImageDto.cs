namespace QwestRooms.BLL.Dtos;

public sealed record ImageDto
{
    public int Id { get; set; }

    public string Path { get; set; } = string.Empty;
}
