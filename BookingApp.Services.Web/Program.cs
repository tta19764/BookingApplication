using BookingApp.Services.Web.Extensions;
using BookingApp.Bll;
using BookingApp.Dal.SqlRepositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
