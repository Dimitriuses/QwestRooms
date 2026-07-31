namespace QwestRooms.DAL.Models;

/// <summary>A picture belonging to a room.</summary>
public class Image
{
    public int Id { get; set; }

    /// <summary>Site-relative path, e.g. <c>/img/rooms/bunker.svg</c>.</summary>
    public required string Path { get; set; }

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;
}
