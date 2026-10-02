using BookingApp.Services.Web.DI;
using BookingApp.Services.Web.Endpoints;
using BookingApp.Services.Web.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    app.ApplyMigrations();

    app.SeedData();
}

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseCustomExceptionManager();

app.MapEndpoints();

app.Run();
