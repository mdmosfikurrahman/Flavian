using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Flavian.Configuration.DependencyInjection;

public static class ConventionRegistrar
{
    public static void RegisterByNamespace(
        IServiceCollection services,
        string interfaceNamespace,
        string implementationNamespace)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in allAssemblies)
        {
            var implementationTypes = assembly.GetTypes()
                .Where(type =>
                    type is
                    {
                        IsClass: true,
                        IsAbstract: false,
                        Namespace: not null
                    } &&
                    type.Namespace.StartsWith(implementationNamespace));

            foreach (var implementation in implementationTypes)
            {
                var matchingInterface = implementation.GetInterfaces()
                    .FirstOrDefault(interfaceType =>
                        interfaceType.Namespace != null &&
                        interfaceType.Namespace.StartsWith(interfaceNamespace) &&
                        $"I{implementation.Name}" == interfaceType.Name);

                if (matchingInterface == null ||
                    matchingInterface.IsGenericTypeDefinition ||
                    implementation.IsGenericTypeDefinition)
                    continue;

                services.AddScoped(matchingInterface, implementation);
            }
        }
    }

    public static void RegisterFluentValidators(IServiceCollection services, string validatorsRootNamespace)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            var candidateTypes = types.Where(t =>
                t is
                {
                    IsClass: true,
                    IsAbstract: false,
                    Namespace: not null
                } &&
                IsUnderRequestRoot(t.Namespace!, validatorsRootNamespace) &&
                EndsWithValidator(t.Namespace!) &&
                !t.IsGenericTypeDefinition);

            foreach (var impl in candidateTypes)
            {
                var validatorInterfaces = impl.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IValidator<>));

                foreach (var service in validatorInterfaces)
                {
                    services.AddScoped(service, impl);
                }
            }
        }

        return;

        static bool EndsWithValidator(string ns) =>
            ns.EndsWith(".Validator", StringComparison.Ordinal);

        static bool IsUnderRequestRoot(string? ns, string root)
        {
            if (string.IsNullOrWhiteSpace(ns)) return false;
            return ns!.Equals(root, StringComparison.Ordinal) ||
                   ns.StartsWith(root + ".", StringComparison.Ordinal);
        }
    }

    public static void RegisterFluentValidatorsFromNamespace(
        IServiceCollection services,
        string validatorsNamespaceRoot)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var seen = new HashSet<(Type service, Type impl)>();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            var candidates = types.Where(t =>
                t is { IsClass: true, IsAbstract: false, Namespace: not null } &&
                IsUnderRoot(t.Namespace!, validatorsNamespaceRoot) &&
                !t.IsGenericTypeDefinition);

            foreach (var impl in candidates)
            {
                var validatorInterfaces = impl.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IValidator<>));

                foreach (var service in validatorInterfaces)
                {
                    if (seen.Add((service, impl)))
                    {
                        services.AddScoped(service, impl);
                    }
                }
            }
        }

        static bool IsUnderRoot(string ns, string root) =>
            ns.Equals(root, StringComparison.Ordinal) ||
            ns.StartsWith(root + ".", StringComparison.Ordinal);
    }
}
