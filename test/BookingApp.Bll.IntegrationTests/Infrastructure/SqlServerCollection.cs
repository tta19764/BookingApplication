namespace BookingApp.Bll.IntegrationTests.Infrastructure;

[CollectionDefinition("SqlServer")]
public sealed class SqlServerCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
