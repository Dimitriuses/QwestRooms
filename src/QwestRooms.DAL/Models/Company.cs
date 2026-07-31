namespace QwestRooms.DAL.Models;

/// <summary>An operator running one or more rooms. The demo dataset has 18 of them.</summary>
public class Company
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Room> Rooms { get; } = new List<Room>();
}
