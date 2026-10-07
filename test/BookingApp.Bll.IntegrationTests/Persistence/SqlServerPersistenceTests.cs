using BookingApp.Bll.Common.Shared.Exceptions;
using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Repositories;
using BookingApp.Services.Web.Services;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll.IntegrationTests.Persistence;

[Collection("SqlServer")]
public sealed class SqlServerPersistenceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<ConferenceHall> CreateHallAsync()
    {
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Зал 'Юнікод' " + Guid.NewGuid().ToString("N")),
            new Capacity(42), new Money(1800.25m, Currency.Uah), [Amenity.Projector]);
        await Halls.AddAsync(hall, Token);
        return hall;
    }

    private static Booking Reservation(Guid hallId, DateTime start, DateTime end) => new(Guid.NewGuid())
    {
        ConferenceHallId = hallId, UserId = SeedDataExtensions.SeededUserId,
        Duration = DateRange.Create(start, end), PriceForPeriod = new Money(1800.25m, Currency.Uah),
        AmenitiesUpCharge = new Money(500m, Currency.Uah), TotalPrice = new Money(2300.25m, Currency.Uah),
        Status = BookingStatus.Reserved, CreatedOnUtc = DateTime.UtcNow
    };

    [Fact]
    public async Task ConcurrentReservations_AllowOnlyOneOverlappingWrite()
    {
        var hall = await CreateHallAsync();
        var start = DateTime.UtcNow.AddDays(10);
        var first = Reservation(hall.Id, start, start.AddHours(2));
        var second = Reservation(hall.Id, start.AddHours(1), start.AddHours(3));
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<ReservationOutcome> Attempt(Booking booking)
        {
            await ready.Task;
            var repository = new BookingRepository(new SqlConnectionFactory(Factory.RuntimeConnectionString), Factory.Services.GetRequiredService<IMapper>());
            return await repository.CreateReservationAsync(booking, Token);
        }
        var attempt1 = Attempt(first);
        var attempt2 = Attempt(second);
        ready.SetResult();
        var outcomes = await Task.WhenAll(attempt1, attempt2);
        outcomes.Should().BeEquivalentTo([ReservationOutcome.Created, ReservationOutcome.Overlap]);
        var stored = new[] { await Bookings.GetByIdAsync(first.Id, Token), await Bookings.GetByIdAsync(second.Id, Token) };
        stored.Should().ContainSingle(booking => booking != null);
    }

    [Fact]
    public async Task AdjacentReservations_AndDifferentHalls_DoNotConflict()
    {
        var hall = await CreateHallAsync();
        var other = await CreateHallAsync();
        var start = DateTime.UtcNow.AddDays(11);
        (await Bookings.CreateReservationAsync(Reservation(hall.Id, start, start.AddHours(1)), Token)).Should().Be(ReservationOutcome.Created);
        (await Bookings.CreateReservationAsync(Reservation(hall.Id, start.AddHours(1), start.AddHours(2)), Token)).Should().Be(ReservationOutcome.Created);
        (await Bookings.CreateReservationAsync(Reservation(other.Id, start, start.AddHours(2)), Token)).Should().Be(ReservationOutcome.Created);
    }

    [Theory]
    [InlineData(BookingStatus.Reserved, true)]
    [InlineData(BookingStatus.Cancelled, false)]
    [InlineData(BookingStatus.Rejected, false)]
    [InlineData(BookingStatus.Completed, false)]
    public async Task OccupancyPolicy_IsConsistentForEveryStatus(BookingStatus status, bool blocks)
    {
        var hall = await CreateHallAsync();
        var start = DateTime.UtcNow.AddDays(12);
        var booking = Reservation(hall.Id, start, start.AddHours(2));
        await Bookings.CreateReservationAsync(booking, Token);
        await using var admin = new SqlConnection(Factory.AdminConnectionString);
        await admin.OpenAsync(Token);
        await using var change = new SqlCommand("UPDATE dbo.bookings SET status=@Status WHERE Id=@Id", admin);
        change.Parameters.AddWithValue("@Status", status.ToString());
        change.Parameters.AddWithValue("@Id", booking.Id);
        await change.ExecuteNonQueryAsync(Token);
        (await Bookings.HasOverlapAsync(hall.Id, booking.Duration, Token)).Should().Be(blocks);
        var available = await Halls.GetAvailableConferenceHallsAsync(booking.Duration, new Capacity(1), Token);
        available.Any(item => item.Id == hall.Id).Should().Be(!blocks);
        (await Bookings.CreateReservationAsync(Reservation(hall.Id, start, start.AddHours(2)), Token))
            .Should().Be(blocks ? ReservationOutcome.Overlap : ReservationOutcome.Created);
    }

    [Fact]
    public async Task FailedInsert_RollsBackHallTimestamp_AndDoesNotInsertBooking()
    {
        var hall = await CreateHallAsync();
        var start = DateTime.UtcNow.AddDays(13);
        var booking = Reservation(hall.Id, start, start.AddHours(2));
        booking.TotalPrice = new Money(-1m, Currency.Uah); // Force an insert constraint failure after the hall UPDATE.
        Func<Task> reserve = async () => await Bookings.CreateReservationAsync(booking, Token);
        var failure = await reserve.Should().ThrowAsync<PersistenceException>();
        failure.Which.Error.Should().Be(PersistenceError.ConstraintViolation);
        failure.Which.InnerException.Should().BeNull();
        (await Halls.GetByIdAsync(hall.Id, Token))!.LastBookedOnUtc.Should().BeNull();
        (await Bookings.GetByIdAsync(booking.Id, Token)).Should().BeNull();
    }

    [Fact]
    public async Task MissingHallOrUser_ReturnsOutcomeWithoutPartialWrite()
    {
        var hall = await CreateHallAsync();
        var start = DateTime.UtcNow.AddDays(14);
        var missingHall = Reservation(Guid.NewGuid(), start, start.AddHours(1));
        (await Bookings.CreateReservationAsync(missingHall, Token)).Should().Be(ReservationOutcome.HallNotFound);
        var missingUser = Reservation(hall.Id, start, start.AddHours(1));
        missingUser.UserId = Guid.NewGuid();
        (await Bookings.CreateReservationAsync(missingUser, Token)).Should().Be(ReservationOutcome.UserNotFound);
        (await Halls.GetByIdAsync(hall.Id, Token))!.LastBookedOnUtc.Should().BeNull();
    }

    [Fact]
    public async Task HallMappingAndEdit_PreserveUnicodeMoneyAndBookingTimestamp()
    {
        var hall = await CreateHallAsync();
        var loaded = (await Halls.GetByIdAsync(hall.Id, Token))!;
        loaded.Name.Should().Be(hall.Name);
        loaded.Price.Should().Be(hall.Price);
        loaded.Amenities.Should().BeEquivalentTo(hall.Amenities);
        loaded.LastBookedOnUtc.Should().BeNull();
        var start = DateTime.UtcNow.AddDays(15);
        var booking = Reservation(hall.Id, start, start.AddHours(1));
        await Bookings.CreateReservationAsync(booking, Token);
        loaded.Name = new Name("Edited");
        (await Halls.UpdateAsync(loaded, Token)).Should().BeTrue();
        (await Halls.GetByIdAsync(hall.Id, Token))!.LastBookedOnUtc.Should().Be(booking.CreatedOnUtc);
        var stored = (await Bookings.GetByIdAsync(booking.Id, Token))!;
        stored.Duration.Should().Be(booking.Duration);
        stored.CreatedOnUtc.Kind.Should().Be(DateTimeKind.Utc);
        stored.TotalPrice.Should().Be(booking.TotalPrice);
        (await Halls.RemoveAsync(hall.Id, Token)).Should().Be(HallRemovalOutcome.HasBookings);
        loaded.Id = Guid.NewGuid();
        (await Halls.UpdateAsync(loaded, Token)).Should().BeFalse();
        (await Halls.RemoveAsync(loaded.Id, Token)).Should().Be(HallRemovalOutcome.NotFound);
    }

    [Fact]
    public async Task Completion_IsBoundedAndRepeatable_AndUsesInclusiveCutoff()
    {
        var hall = await CreateHallAsync();
        var cutoff = DateTime.UtcNow.AddDays(-1);
        var first = Reservation(hall.Id, cutoff.AddHours(-2), cutoff.AddHours(-1));
        var second = Reservation(hall.Id, cutoff.AddHours(-1), cutoff);
        var future = Reservation(hall.Id, cutoff, cutoff.AddHours(1));
        foreach (var booking in new[] { first, second, future }) await Bookings.CreateReservationAsync(booking, Token);
        (await Bookings.CompleteDueAsync(cutoff, 1, Token)).Should().Be(1);
        (await Bookings.CompleteDueAsync(cutoff, 1, Token)).Should().Be(1);
        (await Bookings.CompleteDueAsync(cutoff, 1, Token)).Should().Be(0);
        (await Bookings.GetByIdAsync(second.Id, Token))!.CompletedOnUtc.Should().Be(cutoff);
        (await Bookings.GetByIdAsync(future.Id, Token))!.Status.Should().Be(BookingStatus.Reserved);
    }

    [Fact]
    public async Task CompetingCompletionWorkers_DoNotDoubleCount()
    {
        var hall = await CreateHallAsync();
        var cutoff = DateTime.UtcNow.AddDays(-20);
        await Bookings.CreateReservationAsync(Reservation(hall.Id, cutoff.AddHours(-1), cutoff), Token);
        var repository = new BookingRepository(new SqlConnectionFactory(Factory.RuntimeConnectionString), Factory.Services.GetRequiredService<IMapper>());
        var counts = await Task.WhenAll(repository.CompleteDueAsync(cutoff, 10, Token), repository.CompleteDueAsync(cutoff, 10, Token));
        counts.Sum().Should().Be(1);
    }

    [Fact]
    public async Task Users_RoundTripRolesAndPermissions_AndRollBackInvalidRole()
    {
        using var scope = Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User(Guid.NewGuid(), new FirstName("Іван"), new LastName("Тест"), new Email($"{Guid.NewGuid():N}@booking.local"));
        user.Roles.Add(new Role(1, "Registered"));
        await users.AddAsync(user, Token);
        var loaded = (await users.GetByIdAsync(user.Id, Token))!;
        loaded.FirstName.Should().Be(user.FirstName);
        loaded.Roles.Should().ContainSingle().Which.Permissions.Should().HaveCount(4);
        user.Id = Guid.NewGuid();
        user.Email = new Email($"{Guid.NewGuid():N}@booking.local");
        user.Roles.Add(new Role(999999, "Missing"));
        Func<Task> create = () => users.AddAsync(user, Token);
        await create.Should().ThrowAsync<PersistenceException>();
        (await users.GetByIdAsync(user.Id, Token)).Should().BeNull();
    }

    [Theory]
    [InlineData("SELECT * FROM dbo.bookings")]
    [InlineData("INSERT dbo.roles(Id,name) VALUES(99,N'Unauthorized')")]
    [InlineData("UPDATE dbo.conference_halls SET name=N'Unauthorized'")]
    [InlineData("DELETE dbo.bookings")]
    [InlineData("CREATE TABLE dbo.unauthorized(Id int)")]
    public async Task RuntimeIdentity_CannotAccessTablesOrDdl(string sql)
    {
        await using var connection = new SqlConnection(Factory.RuntimeConnectionString);
        await connection.OpenAsync(Token);
        await using var command = new SqlCommand(sql, connection);
        Func<Task> execute = async () => await command.ExecuteNonQueryAsync(Token);
        var failure = await execute.Should().ThrowAsync<SqlException>();
        failure.Which.Number.Should().BeOneOf(229, 262);
    }

    [Fact]
    public async Task DeploymentRerun_IsNoOp_AndRejectsChangedChecksum()
    {
        await DatabaseInitializer.ApplyAsync(Factory.AdminConnectionString, Token);
        await using var admin = new SqlConnection(Factory.AdminConnectionString);
        await admin.OpenAsync(Token);
        await using var change = new SqlCommand("UPDATE dbo.booking_schema_versions SET checksum=@Checksum OUTPUT deleted.checksum WHERE version=N'002_stored_procedures.sql'", admin);
        change.Parameters.AddWithValue("@Checksum", new string('0', 64));
        var previous = (string)(await change.ExecuteScalarAsync(Token))!;
        try
        {
            Func<Task> deploy = () => DatabaseInitializer.ApplyAsync(Factory.AdminConnectionString, Token);
            await deploy.Should().ThrowAsync<InvalidOperationException>().WithMessage("*changed after deployment*");
        }
        finally
        {
            await using var restore = new SqlCommand("UPDATE dbo.booking_schema_versions SET checksum=@Checksum WHERE version=N'002_stored_procedures.sql'", admin);
            restore.Parameters.AddWithValue("@Checksum", previous);
            await restore.ExecuteNonQueryAsync(Token);
        }
    }

    [Fact]
    public async Task Cancellation_DoesNotPreventSubsequentPooledOperations()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Func<Task> read = async () => await Halls.GetByIdAsync(Guid.NewGuid(), cancelled.Token);
        await read.Should().ThrowAsync<OperationCanceledException>();
        (await Halls.GetListPaginatedAsync(1, 1, Token)).Should().HaveCount(1);
    }
}
