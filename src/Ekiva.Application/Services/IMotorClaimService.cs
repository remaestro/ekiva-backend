using Ekiva.Application.DTOs.Motor;

namespace Ekiva.Application.Services;

/// <summary>
/// Interface pour la gestion des sinistres automobiles
/// </summary>
public interface IMotorClaimService
{
    // Création et consultation
    Task<ClaimDto> CreateClaimAsync(CreateClaimDto request);
    Task<ClaimDto> GetClaimByIdAsync(Guid claimId);
    Task<ClaimDto> GetClaimByNumberAsync(string claimNumber);
    Task<List<ClaimDto>> GetClaimsByPolicyIdAsync(Guid policyId);
    Task<List<ClaimDto>> GetClaimsByClientIdAsync(Guid clientId);
    Task<List<ClaimDto>> GetClaimsByStatusAsync(Core.Entities.ClaimStatus status);
    Task<List<ClaimDto>> GetAllClaimsAsync();

    // Workflow du sinistre
    Task<ClaimDto> SubmitClaimAsync(Guid claimId);
    Task<ClaimDto> StartReviewAsync(Guid claimId, string reviewerUserId);
    Task<ClaimDto> AssignExpertAsync(Guid claimId, AssignExpertDto request, string userId);
    Task<ClaimDto> SubmitExpertiseAsync(Guid claimId, SubmitExpertiseDto request, string userId);
    Task<ClaimDto> ApproveClaimAsync(Guid claimId, ApproveClaimDto request, string userId);
    Task<ClaimDto> RejectClaimAsync(Guid claimId, RejectClaimDto request, string userId);
    Task<ClaimDto> SettleClaimAsync(Guid claimId, SettleClaimDto request, string userId);
    Task<ClaimDto> CloseClaimAsync(Guid claimId, string userId);

    // Documents
    Task<ClaimDocumentDto> UploadDocumentAsync(Guid claimId, string documentType, string fileName, 
        string filePath, string contentType, long fileSize, string userId, string? description = null);
    Task<bool> DeleteDocumentAsync(Guid documentId);
    Task<List<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId);

    // Tiers
    Task<ThirdPartyDto> AddThirdPartyAsync(Guid claimId, CreateThirdPartyDto request);
    Task<bool> RemoveThirdPartyAsync(Guid thirdPartyId);
    Task<ThirdPartyDto> UpdateThirdPartyAsync(Guid thirdPartyId, CreateThirdPartyDto request);

    // Historique
    Task<List<ClaimHistoryDto>> GetClaimHistoryAsync(Guid claimId);

    // Mise à jour
    Task<ClaimDto> UpdateInternalNotesAsync(Guid claimId, string notes, string userId);
    Task<ClaimDto> UpdateClaimAsync(Guid claimId, CreateClaimDto request);
}
