using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Orchestrates recording a health check. Deliberately trivial — this exists
/// to prove the Application layer can depend on a port (IHealthCheckRepository)
/// without knowing about Infrastructure.
/// </summary>
public sealed class RecordHealthCheckHandler
{
    private readonly IHealthCheckRepository _repository;

    public RecordHealthCheckHandler(IHealthCheckRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<HealthCheck> HandleAsync(CancellationToken cancellationToken) =>
        _repository.RecordAsync(cancellationToken);
}
