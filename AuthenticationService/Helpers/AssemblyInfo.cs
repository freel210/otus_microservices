using System.Reflection;

namespace AuthenticationService.Helpers;

public static class AssemblyInfo
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string? AssemblyName = Assembly.GetName().Name;

    private static readonly string AssemblyVersion =
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "UndefinedVersion";

    public static readonly string ProgramNameVersion = $"{AssemblyName} v{AssemblyVersion}";
}
