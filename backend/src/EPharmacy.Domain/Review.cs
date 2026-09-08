namespace EPharmacy.Domain;

public sealed class Review
{
    private Review(Guid id, Guid userId, Guid medicineId, string reviewerDisplayName, int rating, string comment, DateTimeOffset createdAtUtc)
    {
        Id = id;
        UserId = userId;
        MedicineId = medicineId;
        ReviewerDisplayName = reviewerDisplayName;
        Rating = rating;
        Comment = comment;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public Guid MedicineId { get; }

    public string ReviewerDisplayName { get; }

    public int Rating { get; }

    public string Comment { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public static Review Create(Guid id, Guid userId, Guid medicineId, string reviewerDisplayName, int rating, string comment, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine id must not be empty.", nameof(medicineId));
        }

        if (string.IsNullOrWhiteSpace(reviewerDisplayName))
        {
            throw new ArgumentException("Reviewer display name must not be empty.", nameof(reviewerDisplayName));
        }

        if (rating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), rating, "Rating must be between 1 and 5.");
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Comment must not be empty.", nameof(comment));
        }

        return new Review(id, userId, medicineId, reviewerDisplayName, rating, comment, createdAtUtc);
    }
}
