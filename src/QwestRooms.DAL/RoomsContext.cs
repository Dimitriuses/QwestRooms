using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QwestRooms.DAL.Models;

namespace QwestRooms.DAL;

/// <summary>
/// The application's single <see cref="DbContext"/>. It carries both the catalogue tables and the
/// ASP.NET Core Identity ones, which is why it derives from <see cref="IdentityDbContext{TUser}"/>.
/// </summary>
/// <remarks>
/// Lazy loading is not enabled, and there is no proxy package referenced. Every query in the
/// business layer states exactly which columns it wants through a projection, so a navigation
/// property can never quietly become a second query.
/// </remarks>
public class RoomsContext(DbContextOptions<RoomsContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Street> Streets => Set<Street>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>(room =>
        {
            room.Property(r => r.Name).HasMaxLength(120);
            room.Property(r => r.Description).HasMaxLength(400);
            room.Property(r => r.Phone).HasMaxLength(40);
            room.Property(r => r.Email).HasMaxLength(200);
            room.Property(r => r.LogoPath).HasMaxLength(260);

            // Deleting an address or a company that still has rooms is a data-entry mistake, not
            // a cascade: refuse it rather than silently removing the listings.
            room.HasOne(r => r.Address).WithMany(a => a.Rooms)
                .HasForeignKey(r => r.AddressId).OnDelete(DeleteBehavior.Restrict);
            room.HasOne(r => r.Company).WithMany(c => c.Rooms)
                .HasForeignKey(r => r.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Image>(image =>
        {
            image.Property(i => i.Path).HasMaxLength(260);

            // An image has no meaning without its room, so this one does cascade.
            image.HasOne(i => i.Room).WithMany(r => r.Images)
                 .HasForeignKey(i => i.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Address>(address =>
        {
            address.Property(a => a.HouseNumber).HasMaxLength(20);

            address.HasOne(a => a.City).WithMany(c => c.Addresses)
                   .HasForeignKey(a => a.CityId).OnDelete(DeleteBehavior.Restrict);
            address.HasOne(a => a.Country).WithMany(c => c.Addresses)
                   .HasForeignKey(a => a.CountryId).OnDelete(DeleteBehavior.Restrict);
            address.HasOne(a => a.Street).WithMany(s => s.Addresses)
                   .HasForeignKey(a => a.StreetId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<City>().Property(c => c.Name).HasMaxLength(120);
        builder.Entity<Country>().Property(c => c.Name).HasMaxLength(120);
        builder.Entity<Street>().Property(s => s.Name).HasMaxLength(120);
        builder.Entity<Company>().Property(c => c.Name).HasMaxLength(160);
    }
}
