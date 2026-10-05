using Microsoft.Extensions.DependencyInjection;
using SoulCore.Core.Persona;
using SoulCore.Host.Persona;

namespace SoulCore.Host.Hosting.ServiceCollectionExtensions;

internal static class PersonaServiceCollectionExtensions
{
    internal static IServiceCollection AddPersonaRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IPersonaPackStore, PersonaPackStore>();
        services.AddSingleton<PersonaStoreHub>();
        services.AddSingleton<IPersonaStoreHub>(sp => sp.GetRequiredService<PersonaStoreHub>());
        services.AddSingleton<ActivePersonaSession>();
        services.AddSingleton<IPersonaSession>(sp => sp.GetRequiredService<ActivePersonaSession>());
        services.AddSingleton<IPersonaToolPathsResolver, PersonaToolPathsResolver>();
        services.AddHostedService<PersonaSessionHostedService>();
        return services;
    }
}
