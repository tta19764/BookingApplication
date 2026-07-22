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
        dbContext.ChangeTracker.Clear();
        ClearRegisteredRoleNavigationState();
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
        dbContext.ChangeTracker.Clear();
        ClearRegisteredRoleNavigationState();
    }

    private static void SeedUser(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<User>().Any(user => user.Id == SeededUserId))
        {
            return;
        }

        ClearRegisteredRoleNavigationState();

        var user = User.Create(
            SeededUserId,
            new FirstName("Seeded"),
            new LastName("User"),
            new Email("seeded.user@booking.local"));

        ClearUserRoles(user);

        dbContext.Set<User>().Add(user);

        dbContext.Set<Dictionary<string, object>>("user_roles").Add(new Dictionary<string, object>
        {
            ["user_id"] = SeededUserId,
            ["role_id"] = Role.Registered.Id
        });
    }

    private static void ClearRegisteredRoleNavigationState()
    {
        // EF relationship fix-up mutates navigation collections on the static role instance.
        // Clearing them keeps repeated app/test startups from reusing previously tracked users.
        Role.Registered.Users.Clear();
        Role.Registered.Permissions.Clear();
    }

    private static void ClearUserRoles(User user)
    {
        var rolesField = typeof(User).GetField("_roles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (rolesField?.GetValue(user) is List<Role> roles)
        {
            roles.Clear();
        }
    }

    private static void SeedConferenceHalls(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<ConferenceHall>().Any())
        {
            return;
        }

        dbContext.Set<ConferenceHall>().AddRange(
            new ConferenceHall(
                Guid.NewGuid(),
                new Name("Hall A"),
                new Capacity(50),
                new Money(2000m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]),
            new ConferenceHall(
                Guid.NewGuid(),
                new Name("Hall B"),
                new Capacity(100),
                new Money(3500m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]),
            new ConferenceHall(
                Guid.NewGuid(),
                new Name("Hall C"),
                new Capacity(30),
                new Money(1500m, Currency.Uah),
                [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]));
    }
}
