using Ekiva.Core.Entities;

namespace Ekiva.Application.DTOs.Motor;

/// <summary>
/// DTO pour créer un sinistre
/// </summary>
public class CreateClaimDto
{
    public Guid MotorPolicyId { get; set; }
    public DateTime ClaimDate { get; set; }
    public string ClaimLocation { get; set; } = string.Empty;
    public ClaimType ClaimType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Circumstances { get; set; }
    public bool HasInjuries { get; set; }
    public int InjuryCount { get; set; }
    public bool HasPoliceReport { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? PoliceStation { get; set; }
    public decimal ClaimedAmount { get; set; }
    public List<CreateThirdPartyDto>? ThirdParties { get; set; }
}

/// <summary>
/// DTO pour les informations d'un sinistre
/// </summary>
public class ClaimDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid MotorPolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public DateTime ReportedDate { get; set; }
    public string ClaimLocation { get; set; } = string.Empty;
    public ClaimType ClaimType { get; set; }
    public string ClaimTypeText { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Circumstances { get; set; }
    public bool HasInjuries { get; set; }
    public int InjuryCount { get; set; }
    public bool HasPoliceReport { get; set; }
    public string? PoliceReportNumber { get; set; }
    public string? PoliceStation { get; set; }
    public decimal ClaimedAmount { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? Deductible { get; set; }
    public decimal? NetPayableAmount { get; set; }
    public string? AssignedExpert { get; set; }
    public DateTime? ExpertiseDate { get; set; }
    public string? ExpertiseReport { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string? PaymentReference { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? InternalNotes { get; set; }
    public List<ClaimDocumentDto> Documents { get; set; } = new();
    public List<ClaimHistoryDto> History { get; set; } = new();
    public List<ThirdPartyDto> ThirdParties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO pour les documents de sinistre
/// </summary>
public class ClaimDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
    public string? UploadedBy { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO pour l'historique d'un sinistre
/// </summary>
public class ClaimHistoryDto
{
    public Guid Id { get; set; }
    public DateTime ActionDate { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ClaimStatus? OldStatus { get; set; }
    public ClaimStatus? NewStatus { get; set; }
    public string? PerformedBy { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// DTO pour créer un tiers impliqué
/// </summary>
public class CreateThirdPartyDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? VehicleRegistration { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? PolicyNumber { get; set; }
    public bool IsAtFault { get; set; }
    public decimal? FaultPercentage { get; set; }
    public string? DamageDescription { get; set; }
}

/// <summary>
/// DTO pour un tiers impliqué
/// </summary>
public class ThirdPartyDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? VehicleRegistration { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? PolicyNumber { get; set; }
    public bool IsAtFault { get; set; }
    public decimal? FaultPercentage { get; set; }
    public string? DamageDescription { get; set; }
}

/// <summary>
/// DTO pour assigner un expert
/// </summary>
public class AssignExpertDto
{
    public string ExpertName { get; set; } = string.Empty;
    public DateTime ExpertiseDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO pour soumettre un rapport d'expertise
/// </summary>
public class SubmitExpertiseDto
{
    public decimal EstimatedAmount { get; set; }
    public string ExpertiseReport { get; set; } = string.Empty;
    public decimal? RecommendedDeductible { get; set; }
}

/// <summary>
/// DTO pour approuver un sinistre
/// </summary>
public class ApproveClaimDto
{
    public decimal ApprovedAmount { get; set; }
    public decimal Deductible { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// DTO pour rejeter un sinistre
/// </summary>
public class RejectClaimDto
{
    public string RejectionReason { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

/// <summary>
/// DTO pour régler un sinistre
/// </summary>
public class SettleClaimDto
{
    public string PaymentReference { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime SettlementDate { get; set; }
    public string? Comments { get; set; }
}
