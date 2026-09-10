using EPharmacy.Application;
using EPharmacy.Domain;
using Microsoft.EntityFrameworkCore;

namespace EPharmacy.Infrastructure;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _dbContext;

    public ReviewRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Review?> FindByUserAndMedicineAsync(Guid userId, Guid medicineId, CancellationToken cancellationToken) =>
        _dbContext.Reviews.SingleOrDefaultAsync(r => r.UserId == userId && r.MedicineId == medicineId, cancellationToken);

    public async Task AddAsync(Review review, CancellationToken cancellationToken)
    {
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> ListForMedicineAsync(Guid medicineId, CancellationToken cancellationToken)
    {
        var reviews = await _dbContext.Reviews
            .Where(r => r.MedicineId == medicineId)
            .ToListAsync(cancellationToken);

        return reviews.OrderByDescending(r => r.CreatedAtUtc).ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, MedicineRatingSummary>> GetRatingSummariesAsync(
        IReadOnlyCollection<Guid> medicineIds,
        CancellationToken cancellationToken)
    {
        var summaries = await _dbContext.Reviews
            .Where(r => medicineIds.Contains(r.MedicineId))
            .GroupBy(r => r.MedicineId)
            .Select(g => new MedicineRatingSummary(g.Key, g.Average(r => r.Rating), g.Count()))
            .ToListAsync(cancellationToken);

        return summaries.ToDictionary(s => s.MedicineId);
    }
}
