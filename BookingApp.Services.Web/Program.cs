using BookingApp.Services.Web.DI;
using BookingApp.Services.Web.Services.Initialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

// Seed through DI before the HTTP server and background scheduler start. Required scripts are applied first when startup seeding is enabled.
await app.Services.GetRequiredService<StartupDataSeeder>().SeedAsync(app.Lifetime.ApplicationStopping);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseCustomExceptionManager();

app.MapControllers();

app.Run();
