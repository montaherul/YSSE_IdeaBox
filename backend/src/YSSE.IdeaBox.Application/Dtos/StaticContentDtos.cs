using System.ComponentModel.DataAnnotations;

namespace YSSE.IdeaBox.Application.Dtos;

public record StaticContentDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record StaticContentUpsertRequest
{
    [Required, MaxLength(100)]
    public string Key { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Content { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}