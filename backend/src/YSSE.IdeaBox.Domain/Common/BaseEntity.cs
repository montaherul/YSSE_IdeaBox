namespace YSSE.IdeaBox.Domain.Common;

/// <summary>
/// Base type for all persisted domain entities. Uses an integer surrogate key and
/// UTC audit timestamps. Soft-deletable entities additionally expose DeletedAt.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Marker for entities that participate in soft deletion
/// (required for moderated content so counts can be recomputed correctly).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAt { get; set; }
}
