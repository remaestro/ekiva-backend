namespace Ekiva.Application.Services;

public interface IPdfGenerationService
{
    /// <summary>
    /// Générer un PDF de devis Motor Insurance
    /// </summary>
    Task<byte[]> GenerateQuotePdfAsync(Guid quoteId);
    
    /// <summary>
    /// Générer un PDF de police Motor Insurance
    /// </summary>
    Task<byte[]> GeneratePolicyPdfAsync(Guid policyId);
    
    /// <summary>
    /// Générer une attestation d'assurance
    /// </summary>
    Task<byte[]> GenerateInsuranceCertificateAsync(Guid policyId);
    
    /// <summary>
    /// Générer un PDF d'avenant
    /// </summary>
    Task<byte[]> GenerateEndorsementPdfAsync(Guid endorsementId);
}
