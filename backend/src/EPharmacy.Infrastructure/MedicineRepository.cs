using EPharmacy.Application;
using EPharmacy.Domain;
using Microsoft.EntityFrameworkCore;

namespace EPharmacy.Infrastructure;

public sealed class MedicineRepository : IMedicineRepository
{
    private readonly AppDbContext _dbContext;

    public MedicineRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<Medicine>> ListAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Medicines.ToListAsync(cancellationToken);
}
