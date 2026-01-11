using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FileServer.Routes;

[UnconditionalSuppressMessage("Trimming", "IL2026",
    Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
[UnconditionalSuppressMessage("Trimming", "IL2070",
    Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
internal static class RouteMetadataExtensions
{
    private static readonly Lazy<Dictionary<Type, Type>> s_paramsTypeToMetaTypeMap = new(() =>
        GetMetaTypes().ToDictionary(GetParamsType));

    public static void AddMeta(this RouteHandlerBuilder routeHandlerBuilder)
    {
        routeHandlerBuilder.Add(endpointBuilder =>
        {
            Type paramsType = ((MethodInfo)endpointBuilder.Metadata[0]).GetGenericArguments().Single();
            Type metaType = s_paramsTypeToMetaTypeMap.Value[paramsType];
            foreach (Attribute attribute in metaType.GetCustomAttributes())
                endpointBuilder.Metadata.Add(attribute);
        });
    }

    private static IEnumerable<Type> GetMetaTypes() =>
        typeof(Program).Assembly.GetTypes().Where(t =>
            t.IsClass && !t.IsAbstract &&
            t.GetInterfaces().Any(IsMetaInterface));

    private static bool IsMetaInterface(Type @interface) =>
        @interface.IsGenericType &&
        @interface.GetGenericTypeDefinition() == typeof(IRouteMeta<>);

    private static Type GetParamsType(Type metaType) =>
        metaType.GetInterfaces().Single().GenericTypeArguments[0];
}
