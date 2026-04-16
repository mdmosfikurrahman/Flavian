using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flavian.Configuration.DependencyInjection;

internal sealed class GlobalRoutePrefixConvention(IRouteTemplateProvider routeTemplateProvider)
    : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix = new(new RouteAttribute(routeTemplateProvider.Template));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var withRoute = controller.Selectors
                .Where(s => s.AttributeRouteModel != null)
                .ToList();

            foreach (var selector in withRoute)
            {
                selector.AttributeRouteModel =
                    AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel);
            }

            var withoutRoute = controller.Selectors
                .Where(s => s.AttributeRouteModel == null)
                .ToList();

            foreach (var selector in withoutRoute)
            {
                selector.AttributeRouteModel = _routePrefix;
            }
        }
    }
}

public static class MvcGlobalPrefixExtensions
{
    public static IMvcBuilder AddControllersWithGlobalRoutePrefix(
        this IServiceCollection services, string prefix)
    {
        return services.AddControllers(options =>
        {
            options.Conventions.Insert(0, new GlobalRoutePrefixConvention(new RouteAttribute(prefix)));
        });
    }
}
