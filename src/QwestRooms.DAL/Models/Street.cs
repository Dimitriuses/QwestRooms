namespace QwestRooms.DAL.Models;

public class Street
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Address> Addresses { get; } = new List<Address>();
}
