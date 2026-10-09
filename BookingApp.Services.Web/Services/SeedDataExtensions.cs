namespace BookingApp.Services.Web.Services;

/// <summary>Temporary booking identity; reference SQL installs this user until authentication is introduced.</summary>
public static class SeedDataExtensions
{
    public static readonly Guid SeededUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
}
