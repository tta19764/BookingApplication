using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlRepositories;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services.Web.Services;

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
        if (dbContext.Set<UserEntity>().Any(user => user.Id == SeededUserId))
        {
            return;
        }

        ClearRegisteredRoleNavigationState();

        var user = new UserEntity
        {
            Id = SeededUserId,
            FirstName = new FirstName("Seeded"),
            LastName = new LastName("User"),
            Email = new Email("seeded.user@booking.local")
        };

        dbContext.Set<UserEntity>().Add(user);

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

    private static void SeedConferenceHalls(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<ConferenceHallEntity>().Any())
        {
            return;
        }

        dbContext.Set<ConferenceHallEntity>().AddRange(
            CreateHall("Hall A", 50, 2000m),
            CreateHall("Hall B", 100, 3500m),
            CreateHall("Hall C", 30, 1500m));
    }

    private static ConferenceHallEntity CreateHall(string name, int capacity, decimal rate) => new()
    {
        Id = Guid.NewGuid(),
        Name = new Name(name),
        Seats = new Capacity(capacity),
        Price = new Money(rate, Currency.Uah),
        Amenities = [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]
    };
}
