using EPharmacy.Application;
using EPharmacy.Domain;

namespace EPharmacy.Infrastructure;

public sealed class HealthCheckRepository : IHealthCheckRepository
{
    private readonly AppDbContext _dbContext;

    public HealthCheckRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<HealthCheck> RecordAsync(CancellationToken cancellationToken)
    {
        var healthCheck = HealthCheck.CreateNow();

        _dbContext.HealthChecks.Add(healthCheck);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return healthCheck;
    }
}
