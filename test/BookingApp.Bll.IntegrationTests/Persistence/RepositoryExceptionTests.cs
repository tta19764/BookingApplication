using System.Text.RegularExpressions;
using AutoMapper;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using BookingApp.Dal.SqlServerRepositories.Repositories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookingApp.Bll.IntegrationTests.Persistence;

[Collection("SqlServer")]
public sealed class RepositoryExceptionTests(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    private readonly string _database = "RepositoryErrors_" + Guid.NewGuid().ToString("N");
    private string _connectionString = string.Empty;
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private readonly CapturingLogger _logs = new();

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(factory.AdminConnectionString, $"CREATE DATABASE [{_database}]");
        _connectionString = new SqlConnectionStringBuilder(factory.AdminConnectionString) { InitialCatalog = _database }.ConnectionString;
        await DatabaseInitializer.ApplyAsync(_connectionString, Token);
    }

    public async ValueTask DisposeAsync() => await ExecuteAsync(factory.AdminConnectionString,
        $"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}]", CancellationToken.None);

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken? token = null)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(token ?? Token);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(token ?? Token);
    }

    // Preserve the production signature while replacing only the body in this test's isolated database.
    private async Task ReplaceProcedureAsync(string procedure, string body)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(Token);
        await using var read = new SqlCommand("SELECT OBJECT_DEFINITION(OBJECT_ID(@Name))", connection);
        read.Parameters.AddWithValue("@Name", procedure);
        var definition = (string)(await read.ExecuteScalarAsync(Token))!;
        var header = definition[..Regex.Match(definition, @"\bAS\b", RegexOptions.IgnoreCase).Index];
        header = Regex.Replace(header, @"CREATE(?:\s+OR\s+ALTER)?\s+PROCEDURE", "ALTER PROCEDURE", RegexOptions.IgnoreCase);
        await ExecuteAsync(_connectionString, header + " AS BEGIN SET NOCOUNT ON; " + body + " END;");
    }

    private static string Procedure(string repository, bool write) => "[TymchenkoOV].[BookingApp." + ((repository, write) switch
    {
        ("Hall", false) => "hall_get", ("Hall", true) => "hall_create",
        ("Booking", false) => "booking_get", ("Booking", true) => "booking_reserve",
        ("User", false) => "user_get", ("User", true) => "user_create",
        _ => throw new ArgumentException("Unknown test repository")
    }) + "]";

    private async Task InvokeAsync(string repository, bool write, CancellationToken token, int timeout = 30,
        string? connectionString = null)
    {
        var connections = new SqlConnectionFactory(connectionString ?? _connectionString, _logs, timeout);
        var mapper = factory.Services.GetRequiredService<IMapper>();
        var id = Guid.NewGuid();
        switch (repository)
        {
            case "Hall":
                var halls = new ConferenceHallRepository(connections, mapper);
                if (write) await halls.AddAsync(new ConferenceHall(id, new Name("Test"), new Capacity(10),
                    new Money(100m, Currency.Uah), []), token);
                else await halls.GetByIdAsync(id, token);
                break;
            case "Booking":
                var bookings = new BookingRepository(connections, mapper);
                if (write) await bookings.CreateReservationAsync(new Booking(id)
                {
                    ConferenceHallId = Guid.NewGuid(), UserId = Guid.NewGuid(),
                    Duration = DateRange.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)),
                    PriceForPeriod = new Money(100m, Currency.Uah), AmenitiesUpCharge = new Money(0m, Currency.Uah),
                    TotalPrice = new Money(100m, Currency.Uah), Status = BookingStatus.Reserved,
                    CreatedOnUtc = DateTime.UtcNow
                }, token);
                else await bookings.GetByIdAsync(id, token);
                break;
            case "User":
                var users = new UserRepository(connections, mapper);
                if (write) await users.AddAsync(new User(id, new FirstName("Test"), new LastName("User"),
                    new Email("test@example.local")), token);
                else await users.GetByIdAsync(id, token);
                break;
        }
    }

    private void AssertSanitized(PersistenceException error, PersistenceError category, string operation)
    {
        error.Error.Should().Be(category);
        error.Operation.Should().Be(operation);
        error.InnerException.Should().BeNull();
        error.IncidentId.Should().NotBeEmpty();
        _logs.Entries.Should().ContainSingle();
        var log = _logs.Entries.Single();
        log.Level.Should().Be(LogLevel.Error);
        log.Message.Should().Contain(error.IncidentId.ToString()).And.Contain(operation)
            .And.NotContain(_connectionString).And.NotContain("private-row-value");
        error.Message.Should().NotContain("private-row-value");
    }

    [Theory]
    [InlineData("Hall", false, "ConferenceHallRepository.GetByIdAsync")]
    [InlineData("Hall", true, "ConferenceHallRepository.AddAsync")]
    [InlineData("Booking", false, "BookingRepository.GetByIdAsync")]
    [InlineData("Booking", true, "BookingRepository.CreateReservationAsync")]
    [InlineData("User", false, "UserRepository.GetByIdAsync")]
    [InlineData("User", true, "UserRepository.AddAsync")]
    public async Task ReadAndWriteFailures_AreTranslatedAndLoggedOnce(string repository, bool write, string operation)
    {
        await ReplaceProcedureAsync(Procedure(repository, write), "THROW 51011, 'private-row-value', 1;");
        Func<Task> action = () => InvokeAsync(repository, write, Token);
        var failure = await action.Should().ThrowAsync<PersistenceException>();
        AssertSanitized(failure.Which, PersistenceError.Failure, operation);
    }

    [Theory]
    [InlineData("Hall", "ConferenceHallRepository.GetByIdAsync")]
    [InlineData("Booking", "BookingRepository.GetByIdAsync")]
    [InlineData("User", "UserRepository.GetByIdAsync")]
    public async Task MissingSchema_IsTranslated(string repository, string operation)
    {
        await ReplaceProcedureAsync(Procedure(repository, false), "SELECT * FROM dbo.missing_test_table;");
        Func<Task> action = () => InvokeAsync(repository, false, Token);
        var failure = await action.Should().ThrowAsync<PersistenceException>();
        AssertSanitized(failure.Which, PersistenceError.SchemaMismatch, operation);
    }

    [Theory]
    [InlineData("Hall", "ConferenceHallRepository.GetByIdAsync")]
    [InlineData("Booking", "BookingRepository.GetByIdAsync")]
    [InlineData("User", "UserRepository.GetByIdAsync")]
    public async Task CommandTimeout_IsTranslated(string repository, string operation)
    {
        await ReplaceProcedureAsync(Procedure(repository, false), "WAITFOR DELAY '00:00:05';");
        Func<Task> action = () => InvokeAsync(repository, false, Token, timeout: 1);
        var failure = await action.Should().ThrowAsync<PersistenceException>();
        AssertSanitized(failure.Which, PersistenceError.Timeout, operation);
    }

    [Theory]
    [InlineData("Hall")]
    [InlineData("Booking")]
    [InlineData("User")]
    public async Task ConnectionFailure_IsTranslatedAndLoggedOnce(string repository)
    {
        var invalid = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "Missing_" + Guid.NewGuid().ToString("N"), ConnectTimeout = 2, ConnectRetryCount = 0 };
        Func<Task> action = () => InvokeAsync(repository, false, Token, connectionString: invalid.ConnectionString);
        var failure = await action.Should().ThrowAsync<PersistenceException>();
        AssertSanitized(failure.Which, PersistenceError.Unavailable, "OpenConnection");
    }

    [Theory]
    [InlineData("Hall")]
    [InlineData("Booking")]
    [InlineData("User")]
    public async Task Cancellation_RemainsCancellationWithoutErrorLog(string repository)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        cancellation.Cancel();
        Func<Task> action = () => InvokeAsync(repository, false, cancellation.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();
        _logs.Entries.Should().BeEmpty();
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Entries.Add((level, formatter(state, exception)));
    }
}
