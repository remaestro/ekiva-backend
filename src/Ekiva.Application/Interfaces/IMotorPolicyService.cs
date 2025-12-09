using Ekiva.Application.DTOs.Motor;

namespace Ekiva.Application.Services;

public interface IMotorPolicyService
{
    /// <summary>
    /// Convertir un devis accepté en police d'assurance
    /// </summary>
    Task<MotorPolicyDto> ConvertQuoteToPolicyAsync(Guid quoteId);
    
    /// <summary>
    /// Obtenir une police par ID
    /// </summary>
    Task<MotorPolicyDto> GetPolicyByIdAsync(Guid policyId);
    
    /// <summary>
    /// Obtenir toutes les polices d'un client
    /// </summary>
    Task<List<MotorPolicyDto>> GetPoliciesByClientIdAsync(Guid clientId);
    
    /// <summary>
    /// Activer une police (après paiement)
    /// </summary>
    Task<MotorPolicyDto> ActivatePolicyAsync(Guid policyId, string paymentReference);
    
    /// <summary>
    /// Suspendre une police
    /// </summary>
    Task<MotorPolicyDto> SuspendPolicyAsync(Guid policyId, string reason);
    
    /// <summary>
    /// Annuler une police
    /// </summary>
    Task<MotorPolicyDto> CancelPolicyAsync(Guid policyId, string reason);
    
    /// <summary>
    /// Créer un avenant (endorsement)
    /// </summary>
    Task<MotorPolicyEndorsementDto> CreateEndorsementAsync(Guid policyId, CreateEndorsementDto request);
    
    /// <summary>
    /// Obtenir les avenants d'une police
    /// </summary>
    Task<List<MotorPolicyEndorsementDto>> GetPolicyEndorsementsAsync(Guid policyId);
}
