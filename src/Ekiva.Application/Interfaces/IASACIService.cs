using Ekiva.Application.DTOs.ASACI;

namespace Ekiva.Application.Interfaces;

/// <summary>
/// Interface pour le service ASACI
/// Gère la création, vérification et gestion des attestations d'assurance
/// </summary>
public interface IASACIService
{
    /// <summary>
    /// Créer une nouvelle attestation ASACI
    /// </summary>
    Task<CertificateResponse> CreateCertificateAsync(CreateCertificateRequest request);

    /// <summary>
    /// Vérifier la validité d'une attestation
    /// </summary>
    Task<VerificationResult> VerifyCertificateAsync(VerifyCertificateRequest request);

    /// <summary>
    /// Obtenir une attestation par numéro
    /// </summary>
    Task<CertificateResponse?> GetCertificateByNumberAsync(string certificateNumber);

    /// <summary>
    /// Obtenir toutes les attestations
    /// </summary>
    Task<List<CertificateResponse>> GetAllCertificatesAsync();

    /// <summary>
    /// Obtenir toutes les attestations d'une police
    /// </summary>
    Task<List<CertificateResponse>> GetCertificatesByPolicyIdAsync(Guid policyId);

    /// <summary>
    /// Suspendre une attestation
    /// </summary>
    Task<CertificateResponse> SuspendCertificateAsync(string certificateNumber, string reason);

    /// <summary>
    /// Annuler une attestation
    /// </summary>
    Task<CertificateResponse> CancelCertificateAsync(string certificateNumber, string reason);

    /// <summary>
    /// Réactiver une attestation suspendue
    /// </summary>
    Task<CertificateResponse> ReactivateCertificateAsync(string certificateNumber);

    /// <summary>
    /// Obtenir les statistiques des attestations
    /// </summary>
    Task<CertificateStats> GetStatisticsAsync();

    /// <summary>
    /// Obtenir les attestations qui expirent bientôt
    /// </summary>
    Task<List<CertificateResponse>> GetExpiringCertificatesAsync(int daysThreshold = 30);
}
