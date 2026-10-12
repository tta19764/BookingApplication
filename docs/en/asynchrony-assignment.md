# Asynchrony assignment

The application implements the lecture requirements in five commit slices:

1. Bookings: XML documentation for controller actions, interface methods, request properties, and response properties.
2. Conference halls: the same documentation coverage for hall queries and mutations.
3. Reports: documentation for the summary endpoint, reporting interface, and summary properties.
4. Shared request processing: request cancellation reaches middleware, JSON writes, event handlers, and SQL callbacks. Correlation logging remains active throughout asynchronous processing. The host uses `RunAsync`.
5. Verification: generated XML files feed Swagger, and architecture tests enforce cancellation and documentation coverage. Swagger uses the Microsoft.OpenApi dependency selected by Swashbuckle instead of an incompatible major-version override.

Controller tokens flow through managers and repositories to SQL connection opening, command execution, reader operations, and transaction commits. Script reading already uses `ReadToEndAsync` with cancellation. The source audit found no blocking database operations or `.Result`/`.Wait()` calls in application code.

Middleware registration explicitly passes `HttpContext.RequestAborted`; it does not ask dependency injection to resolve a `CancellationToken`. Aborted requests propagate cancellation without attempting to write an error response. CLR-generated entry points, third-party APIs, and test-framework entry points are outside the application-method cancellation contract. Database deployment deliberately releases its session lock with `CancellationToken.None` in cleanup, even when deployment was cancelled.

DTO records retain their positional constructors and JSON shape. Explicit properties provide XML member summaries that Swagger can display. Architecture tests check the generated XML files for controller actions, application interface methods, and DTO properties. CS1591 is suppressed for other public symbols outside this assignment's documentation scope; malformed XML documentation still produces compiler diagnostics.

## Verification

```powershell
dotnet build BookingApplicationSolution.sln
dotnet test --project test/BookingApp.Services.Web.UnitTests/BookingApp.Services.Web.UnitTests.csproj
dotnet test --project test/BookingApp.Bll.UnitTests/BookingApp.Bll.UnitTests.csproj
dotnet test --project test/BookingApp.Bll.Common.UnitTests/BookingApp.Bll.Common.UnitTests.csproj
dotnet test --project test/BookingApp.ArchitectureTests/BookingApp.ArchitectureTests.csproj
```

The repository selects Microsoft.Testing.Platform in `global.json`, so test commands use `--project`. Database integration suites require a running Docker engine and an ephemeral SQL Server container.
