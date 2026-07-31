using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using QwestRooms.DAL.Seeding;

var builder = WebApplication.CreateBuilder(args);

// SQLite rather than SQL Server LocalDB. The 2019 version could only run on Windows with a
// LocalDB instance installed; the file-based database means `dotnet run` works on a clean
// machine on any of the three platforms, and CI can start the real application on Linux.
var connectionString = builder.Configuration.GetConnectionString("RoomsContext")
                       ?? "Data Source=qwestrooms.db";

builder.Services.AddDbContext<RoomsContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IAddressesService, AddressesService>();

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        // The same rules the RegisterViewModel annotations describe, so client-side hints and the
        // server agree. Change one and the other needs the same change.
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<RoomsContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrate and seed once, at startup. In 2019 this was an EF6 initializer, so the schema was
// created by whichever HTTP request happened to arrive first.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RoomsContext>();
    var rooms = await DatabaseSeeder.InitialiseAsync(context).ConfigureAwait(false);
    app.Logger.LogInformation("Catalogue ready with {RoomCount} rooms.", rooms);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Room/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Room}/{action=Index}/{id?}");

// A readiness probe that touches the database, so "the site started" and "the site can serve the
// catalogue" are not the same claim. The Linux CI job asserts on the room count it returns.
app.MapGet("/healthz", async (RoomsContext context, CancellationToken cancellationToken) =>
    Results.Ok(new
    {
        status = "ok",
        rooms = await context.Rooms.CountAsync(cancellationToken).ConfigureAwait(false)
    }));

await app.RunAsync().ConfigureAwait(false);

/// <summary>Named so the integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program
{
    private Program()
    {
    }
}
