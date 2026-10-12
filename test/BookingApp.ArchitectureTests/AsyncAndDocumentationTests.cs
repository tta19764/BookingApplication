using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Services.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.ArchitectureTests;

public sealed class AsyncAndDocumentationTests
{
    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(IBookingManager).Assembly,
        typeof(BookingManager).Assembly,
        typeof(SqlConnectionFactory).Assembly,
        typeof(BookingsController).Assembly
    ];

    [Fact]
    public void AsyncApplicationMethods_MustAcceptCancellationToken()
    {
        var violations = ApplicationAssemblies.SelectMany(assembly => assembly.GetTypes())
            // CLR-generated entry points and state machines are outside application contracts.
            .Where(type => type.Namespace?.StartsWith("BookingApp.", StringComparison.Ordinal) == true)
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute)))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(method => !method.IsDefined(typeof(CompilerGeneratedAttribute)))
            .Where(method => IsAsyncReturnType(method.ReturnType))
            .Where(method => !method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken)))
            .Select(method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .ToArray();

        violations.Should().BeEmpty("every application async method must support cancellation");
    }

    [Fact]
    public void ApiActionsAndInterfaceMethods_MustHaveXmlSummaries()
    {
        var violations = new List<string>();
        foreach (var assembly in new[] { typeof(IBookingManager).Assembly, typeof(BookingsController).Assembly })
        {
            var documented = ReadDocumentedMembers(assembly);
            var methods = assembly.GetExportedTypes()
                .Where(type => type.IsInterface || typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => !method.IsSpecialName);
            foreach (var method in methods)
            {
                var prefix = $"M:{method.DeclaringType!.FullName}.{method.Name}";
                if (!documented.Any(name => name == prefix || name.StartsWith(prefix + "(", StringComparison.Ordinal)
                    || name.StartsWith(prefix + "``", StringComparison.Ordinal)))
                    violations.Add(prefix);
            }
        }

        violations.Should().BeEmpty("API actions and interface methods need XML documentation");
    }

    [Fact]
    public void DtoProperties_MustHaveXmlSummaries()
    {
        var violations = new List<string>();
        foreach (var assembly in new[] { typeof(IBookingManager).Assembly, typeof(BookingsController).Assembly })
        {
            var documented = ReadDocumentedMembers(assembly);
            var types = assembly.GetExportedTypes().Where(type =>
                type.Namespace?.StartsWith("BookingApp.Services.Web.Dtos", StringComparison.Ordinal) == true
                || type.Name.EndsWith("Model", StringComparison.Ordinal)
                || type.Name is "PricingDetails" or "Error" or "ValidationError" or "DatabaseInspection" or "SeedResult");
            foreach (var property in types.SelectMany(type => type.GetProperties(BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.DeclaredOnly)))
            {
                var name = $"P:{property.DeclaringType!.FullName}.{property.Name}";
                if (!documented.Contains(name)) violations.Add(name);
            }
        }

        violations.Should().BeEmpty("DTO members need XML documentation, including generated record properties");
    }

    private static HashSet<string> ReadDocumentedMembers(Assembly assembly)
    {
        var path = Path.ChangeExtension(assembly.Location, ".xml");
        File.Exists(path).Should().BeTrue("XML documentation must be generated and copied with the assembly");
        return XDocument.Load(path).Descendants("member")
            .Where(member => !string.IsNullOrWhiteSpace(member.Element("summary")?.Value))
            .Select(member => member.Attribute("name")!.Value).ToHashSet();
    }

    private static bool IsAsyncReturnType(Type type) => typeof(Task).IsAssignableFrom(type)
        || type == typeof(ValueTask)
        || type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ValueTask<>)
            || type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
}
