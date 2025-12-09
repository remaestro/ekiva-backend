using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

/// <summary>
/// Statut d'une attestation ASACI
/// </summary>
public enum CertificateStatus
{
    Active,      // Attestation active et valide
    Suspended,   // Suspendue temporairement
    Cancelled,   // Annulée définitivement
    Expired      // Expirée
}

/// <summary>
/// Attestation d'assurance ASACI
/// Représente un certificat d'assurance automobile enregistré auprès de l'ASACI
/// </summary>
public class ASACICertificate : BaseEntity
{
    /// <summary>
    /// Numéro unique de l'attestation ASACI
    /// Format: ASACI-YYYY-XXXXXXXX
    /// </summary>
    public string CertificateNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID de la police d'assurance associée
    /// </summary>
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }

    /// <summary>
    /// Informations du véhicule
    /// </summary>
    public string VehicleRegistration { get; set; } = string.Empty; // Immatriculation
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;

    /// <summary>
    /// Informations de l'assuré
    /// </summary>
    public string PolicyHolderName { get; set; } = string.Empty;
    public string PolicyHolderAddress { get; set; } = string.Empty;
    public string PolicyHolderPhone { get; set; } = string.Empty;

    /// <summary>
    /// Dates de validité
    /// </summary>
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Statut actuel de l'attestation
    /// </summary>
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;

    /// <summary>
    /// Date de génération de l'attestation
    /// </summary>
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière modification du statut
    /// </summary>
    public DateTime? StatusChangedDate { get; set; }

    /// <summary>
    /// Raison de suspension/annulation
    /// </summary>
    public string? StatusChangeReason { get; set; }

    /// <summary>
    /// Informations complémentaires
    /// </summary>
    public string InsuranceCompanyName { get; set; } = "EKIVA";
    public string InsuranceCompanyCode { get; set; } = "EKV";
    public string ProductType { get; set; } = "Motor"; // Motor, Liability, etc.
    public string CoverageType { get; set; } = string.Empty; // Third Party, Comprehensive, etc.

    /// <summary>
    /// Données brutes de la réponse ASACI (pour audit)
    /// </summary>
    public string? ASACIResponseData { get; set; }

    /// <summary>
    /// Vérifie si l'attestation est valide
    /// </summary>
    public bool IsValid()
    {
        return Status == CertificateStatus.Active 
               && DateTime.UtcNow >= EffectiveDate 
               && DateTime.UtcNow <= ExpiryDate;
    }

    /// <summary>
    /// Vérifie si l'attestation est expirée
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiryDate;
    }

    /// <summary>
    /// Calcule le nombre de jours restants avant expiration
    /// </summary>
    public int DaysUntilExpiry()
    {
        return (ExpiryDate - DateTime.UtcNow).Days;
    }
}
