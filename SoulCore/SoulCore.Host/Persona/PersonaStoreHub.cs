using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoulCore.Config;
using SoulCore.Core.Abstractions;
using SoulCore.Core.Charter;
using SoulCore.Memory;

namespace SoulCore.Host.Persona;

/// <summary>
/// Opens / tears down per-persona quarantined SQLite (+ vector dir) stores.
/// Single-active v1; accessors remain keyed by <c>personaId</c>.
/// </summary>
public sealed class PersonaStoreHub : IPersonaStoreHub
{
    private readonly string _personasRoot;
    private readonly ILogger<PersonaStoreHub> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private PersonaStoreBundle? _active;
    private bool _disposed;

    public PersonaStoreHub(IOptions<PersonaOptions> options, ILogger<PersonaStoreHub> logger)
    {
        _personasRoot = (options?.Value ?? new PersonaOptions()).ResolveRootDirectory();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Test helper: construct against an explicit personas root.</summary>
    public PersonaStoreHub(string personasRoot, ILogger<PersonaStoreHub>? logger = null)
    {
        _personasRoot = Path.GetFullPath(personasRoot);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PersonaStoreHub>.Instance;
    }

    public string PersonasRoot => _personasRoot;

    public string ActivePersonaId
    {
        get
        {
            ThrowIfDisposed();
            var bundle = _active
                ?? throw new InvalidOperationException("PersonaStoreHub has no active persona yet. Call SwitchActiveAsync first.");
            return bundle.PersonaId;
        }
    }

    public string ResolveMemoryDbPath(string personaId) =>
        PersonaMemoryPaths.ResolveMemoryDbPath(_personasRoot, personaId);

    public string ResolveVectorDirectory(string personaId) =>
        PersonaMemoryPaths.ResolveVectorDirectory(_personasRoot, personaId);

    public async Task SwitchActiveAsync(string personaId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(personaId);
        var id = PersonaMemoryPaths.NormalizePersonaId(personaId);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_active is not null
                && string.Equals(_active.PersonaId, id, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var next = new PersonaStoreBundle(id, _personasRoot);
            var previous = _active;
            _active = next;

            if (previous is not null)
            {
                try
                {
                    await previous.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Persona store tear-down failed for {PersonaId} (new active={NewId})",
                        previous.PersonaId,
                        id);
                }
            }

            _logger.LogInformation(
                "Persona stores active={PersonaId} db={DbPath} vectorDir={VectorDir}",
                id,
                next.DatabasePath,
                next.VectorDirectory);
        }
        finally
        {
            _gate.Release();
        }
    }

    public IMemoryStore GetMemoryStore(string personaId) => RequireBundle(personaId).Episodic;

    public IMemoryStats GetMemoryStats(string personaId) => RequireBundle(personaId).Episodic;

    public IEmotionState GetEmotionState(string personaId) => RequireBundle(personaId).Emotion;

    public IVictoriaJournalStore GetJournalStore(string personaId) => RequireBundle(personaId).Journals;

    public IVictoriaTaskStore GetTaskStore(string personaId) => RequireBundle(personaId).Tasks;

    public IVictoriaWorkflowStore GetWorkflowStore(string personaId) => RequireBundle(personaId).Workflows;

    public IChatTranscriptStore GetTranscriptStore(string personaId) => RequireBundle(personaId).Transcript;

    public ICharter GetCharter(string personaId) => RequireBundle(personaId).Charter;

    public CharterService GetCharterService(string personaId) => RequireBundle(personaId).Charter;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;
            _disposed = true;
            if (_active is not null)
            {
                await _active.DisposeAsync().ConfigureAwait(false);
                _active = null;
            }
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }

    private PersonaStoreBundle RequireBundle(string personaId)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(personaId);
        var id = PersonaMemoryPaths.NormalizePersonaId(personaId);
        var bundle = _active
            ?? throw new InvalidOperationException("PersonaStoreHub has no active persona yet. Call SwitchActiveAsync first.");

        if (!string.Equals(bundle.PersonaId, id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Persona '{id}' is not the active store bundle (active='{bundle.PersonaId}'). " +
                "v1 is single-active; call SwitchActiveAsync before accessing another persona's stores.");
        }

        return bundle;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PersonaStoreHub));
    }
}
