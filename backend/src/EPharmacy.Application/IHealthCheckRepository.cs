using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Port for recording health checks. Implemented by Infrastructure.
/// </summary>
public interface IHealthCheckRepository
{
    Task<HealthCheck> RecordAsync(CancellationToken cancellationToken);
}
