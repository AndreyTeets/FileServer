using System.Diagnostics.CodeAnalysis;

namespace FileServer.Routes;

[UnconditionalSuppressMessage("Trimming", "IL2026",
    Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
[UnconditionalSuppressMessage("Trimming", "IL2070",
    Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
internal static class RouteHandlersLocator
{
    public static IEnumerable<(Type interfaceType, Type implementingType)> GetAll() =>
        GetAllUnordered().OrderBy(x => GetParamsType(x.interfaceType).FullName);

    private static IEnumerable<(Type interfaceType, Type implementingType)> GetAllUnordered()
    {
        foreach (Type implementingType in GetHandlerTypes())
        {
            foreach (Type interfaceType in GetImplementedHandlerInterfaces(implementingType))
                yield return (interfaceType, implementingType);
        }
    }

    private static IEnumerable<Type> GetHandlerTypes() =>
        typeof(Program).Assembly.GetTypes().Where(t =>
            t.IsClass && !t.IsAbstract &&
            t.GetInterfaces().Any(IsHandlerInterface));

    private static IEnumerable<Type> GetImplementedHandlerInterfaces(Type type) =>
        type.GetInterfaces().Where(IsHandlerInterface);

    private static bool IsHandlerInterface(Type @interface) =>
        @interface.IsGenericType &&
        @interface.GetGenericTypeDefinition() == typeof(IRouteHandler<>);

    private static Type GetParamsType(Type interfaceType) =>
        interfaceType.GenericTypeArguments[0];
}
