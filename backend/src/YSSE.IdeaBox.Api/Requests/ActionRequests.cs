using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Api.Requests;

/// <summary>Body for POST conversations/{id}/status.</summary>
public record ConversationStatusRequest
{
    [Required]
    public ConversationStatus Status { get; init; }
}

/// <summary>Body for POST marketplace/{id}/status.</summary>
public record ListingStatusRequest
{
    [Required]
    public ListingStatus Status { get; init; }
}

/// <summary>Body for POST reservations/{id}/respond.</summary>
public record ReservationRespondRequest
{
    public bool Accept { get; init; }
}
