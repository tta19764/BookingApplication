using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookingApp.Api.OpenApi;

public sealed  class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider): IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = $"BookingApp.Api v{description.ApiVersion}",
            Version = description.ApiVersion.ToString()
        };
        
        if (description.IsDeprecated)
        {
            info.Description = "This version has been deprecated and is no longer supported.";
        }
        
        return info;
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }
}