namespace SoulCore.Config;

/// <summary>
/// Non-secret Memory SQLite knobs for CLI / tests.
/// Host live stores use <see cref="PersonaMemoryPaths"/> (PROP-15.2) — not this path.
/// Default CLI DB lives under local app data — never LLMOD Data/.
/// </summary>
public sealed class MemoryOptions
{
    public const string SectionName = "Memory";

    /// <summary>
    /// Absolute or relative path to the SQLite file for CLI evidence tools.
    /// Empty = %LOCALAPPDATA%/SoulCore/memory/soulcore_memory.db.
    /// Host chat/SoulLoop/charter use per-persona paths under personas/{id}/memory/.
    /// </summary>
    public string DbPath { get; set; } = string.Empty;

    public static string ResolveDefaultDbPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "SoulCore", "memory", "soulcore_memory.db");
    }

    public string ResolveDbPath()
    {
        if (string.IsNullOrWhiteSpace(DbPath))
            return ResolveDefaultDbPath();

        return Path.GetFullPath(Environment.ExpandEnvironmentVariables(DbPath.Trim()));
    }
}
