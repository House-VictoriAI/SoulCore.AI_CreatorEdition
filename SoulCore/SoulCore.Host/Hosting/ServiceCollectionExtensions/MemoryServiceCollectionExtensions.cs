using Microsoft.Extensions.DependencyInjection;
using SoulCore.Config;
using SoulCore.Inference.Tooling;
using SoulCore.Core.Abstractions;
using SoulCore.Core.Safety;
using SoulCore.Inference.Tools;
using SoulCore.Host.Persona;
using SoulCore.Memory;

namespace SoulCore.Host.Hosting.ServiceCollectionExtensions;

internal static class MemoryServiceCollectionExtensions
{
    internal static IServiceCollection AddMemory(
        this IServiceCollection services,
        SafetyOptions safetyOptions)
    {
        // PROP-15.2: stores are persona-quarantined via IPersonaStoreHub.
        // Register AddPersonaRuntime() before AddMemory so the hub exists.
        services.AddSingleton<IMemoryStore, PersonaScopedMemoryStore>();
        services.AddSingleton<IMemoryStats, PersonaScopedMemoryStats>();
        services.AddSingleton<IEmotionState, PersonaScopedEmotionState>();
        services.AddSingleton<IVictoriaTaskStore, PersonaScopedTaskStore>();
        services.AddSingleton<IVictoriaWorkflowStore, PersonaScopedWorkflowStore>();
        services.AddSingleton<IVictoriaJournalStore, PersonaScopedJournalStore>();
        services.AddSingleton<IChatTranscriptStore, PersonaScopedTranscriptStore>();
        services.AddSingleton<ICharter, PersonaScopedCharter>();

        // Safety / spend layer (BED-080 libs wired by BED-082; TASK-102 hard gate on CapExceeded).
        services.AddSingleton<DriftWatcher>(_ => new DriftWatcher(safetyOptions.DriftSloMinutes));
        services.AddSingleton<SpendMeter>(_ => new SpendMeter(
            safetyOptions.InputTokenRatePer1K,
            safetyOptions.OutputTokenRatePer1K,
            safetyOptions.MonthlyCapUsd,
            safetyOptions.MonthlyTokenCap));

        // Memory tools (BED-131): recall_memory + store_memory wrap IMemoryStore so
        // the model can decide to recall a specific memory or store a new one within
        // a turn (in addition to the preamble-injected baseline recall). Registered as
        // ITool singletons — ToolRegistry collects them via IEnumerable<ITool>.
        services.AddSingleton<ITool, RecallMemoryTool>();
        services.AddSingleton<ITool, StoreMemoryTool>();

        return services;
    }
}
