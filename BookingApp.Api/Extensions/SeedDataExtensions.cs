using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;
using BookingApp.Domain.Users;
using BookingApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Api.Extensions;

/// <summary>
/// Seeds baseline data required while authentication and administration are out of scope.
/// </summary>
public static class SeedDataExtensions
{
    /// <summary>
    /// Stable user identifier used by booking endpoints until real authentication is added.
    /// </summary>
    public static readonly Guid SeededUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Creates the database if needed and inserts halls, permissions, role mappings, and the seeded user.
    /// </summary>
    public static void SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.EnsureCreated();

        SeedRolesAndPermissions(dbContext);
        SeedUser(dbContext);
        SeedConferenceHalls(dbContext);

        dbContext.SaveChanges();
    }

    private static void SeedRolesAndPermissions(ApplicationDbContext dbContext)
    {
        if (!dbContext.Set<Role>().Any(role => role.Id == Role.Registered.Id))
        {
            dbContext.Set<Role>().Add(Role.Registered);
        }

        var permissions = new[]
        {
            Permission.ConferenceHallRead,
            Permission.ConferenceHallWrite,
            Permission.BookingRead,
            Permission.BookingWrite
        };

        foreach (var permission in permissions)
        {
            if (!dbContext.Set<Permission>().Any(existing => existing.Id == permission.Id))
            {
                dbContext.Set<Permission>().Add(permission);
            }
        }

        foreach (var permission in permissions)
        {
            var exists = dbContext.Set<RolePermission>().Any(rolePermission =>
                rolePermission.RoleId == Role.Registered.Id &&
                rolePermission.PermissionId == permission.Id);

            if (exists)
            {
                continue;
            }

            dbContext.Set<RolePermission>().Add(new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = permission.Id
            });
        }

        // Persist catalog rows before creating the user-role join row.
        dbContext.SaveChanges();
    }

    private static void SeedUser(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<User>().Any(user => user.Id == SeededUserId))
        {
            return;
        }

        var user = User.Create(
            SeededUserId,
            new FirstName("Seeded"),
            new LastName("User"),
            new Email("seeded.user@booking.local"));

        if (dbContext.Entry(Role.Registered).State == EntityState.Detached)
        {
            dbContext.Attach(Role.Registered);
        }

        dbContext.Set<User>().Add(user);
    }

    private static void SeedConferenceHalls(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<ConferenceHall>().Any())
        {
            return;
        }

        dbContext.Set<ConferenceHall>().AddRange(
            new ConferenceHall(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                new Name("Hall A"),
                new Capacity(50),
                new Money(2000m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]),
            new ConferenceHall(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                new Name("Hall B"),
                new Capacity(100),
                new Money(3500m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]),
            new ConferenceHall(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                new Name("Hall C"),
                new Capacity(30),
                new Money(1500m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]));
    }
}
