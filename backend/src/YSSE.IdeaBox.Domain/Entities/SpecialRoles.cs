using YSSE.IdeaBox.Domain.Common;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>
/// Idea Helper submission on behalf of someone without a device/literacy (FR-B29).
/// Requires the creator's recorded consent and confirmation of ownership.
/// </summary>
public class HelperAssistedSubmission : BaseEntity
{
    public int HelperUserId { get; set; }

    public int IdeaId { get; set; }

    public int CreatorUserId { get; set; }

    public string ConsentMethod { get; set; } = string.Empty;

    public bool CreatorConfirmed { get; set; }

    public DateTime? CreatorConfirmedAt { get; set; }
}

/// <summary>Verified guardian relationship for a minor account (FR-B32).</summary>
public class GuardianLink : BaseEntity
{
    public int GuardianUserId { get; set; }

    public int MinorUserId { get; set; }

    public bool IsVerified { get; set; }

    public bool CanBlockInquiries { get; set; } = true;

    public DateTime? VerifiedAt { get; set; }
}

/// <summary>Verified partner organization (school, NGO, incubator) (FR-B / 4.4).</summary>
public class OrganizationPartner : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ICollection<OrganizationPartnerMember> Members { get; set; } = new List<OrganizationPartnerMember>();
}

public class OrganizationPartnerMember : BaseEntity
{
    public int OrganizationPartnerId { get; set; }

    public OrganizationPartner OrganizationPartner { get; set; } = null!;

    public int UserId { get; set; }
}
