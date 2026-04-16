using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Flavian.Configuration.Middlewares;

namespace Flavian.Configuration.DependencyInjection;

public static class WebApplicationExtensions
{
    public static void UseProjectConfiguration(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseRateLimiter();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseSwaggerDocs();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGraphQL("/api/v1/graphql");
    }

    private static void UseSwaggerDocs(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger(c => { c.RouteTemplate = "api/swagger/{documentName}/swagger.json"; });

        app.UseSwaggerUI(options =>
        {
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/api/swagger/{desc.GroupName}/swagger.json",
                    $"Flavian API {desc.GroupName.ToUpperInvariant()}"
                );
            }

            options.RoutePrefix = "api/swagger";
        });
    }
}
