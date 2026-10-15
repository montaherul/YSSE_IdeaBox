using YSSE.IdeaBox.Domain.Common;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>
/// Static page content for CMS-managed pages (About, Guidelines, Privacy, Terms, Contact, etc.)
/// </summary>
public class StaticContent : BaseEntity, ISoftDeletable
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}