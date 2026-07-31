namespace QwestRooms.DAL.Models;

public class Country
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Address> Addresses { get; } = new List<Address>();
}
