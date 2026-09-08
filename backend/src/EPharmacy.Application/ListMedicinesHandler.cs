using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Orchestrates listing the medicine catalog. Deliberately trivial — this exists
/// to prove the Application layer can depend on a port (IMedicineRepository)
/// without knowing about Infrastructure.
/// </summary>
public sealed class ListMedicinesHandler
{
    private readonly IMedicineRepository _repository;

    public ListMedicinesHandler(IMedicineRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IReadOnlyList<Medicine>> HandleAsync(CancellationToken cancellationToken) =>
        _repository.ListAllAsync(cancellationToken);
}
