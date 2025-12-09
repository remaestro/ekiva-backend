using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

/// <summary>
/// Type de sinistre automobile
/// </summary>
public enum ClaimType
{
    Accident = 1,        // Accident de la circulation
    Theft = 2,           // Vol
    Fire = 3,            // Incendie
    Vandalism = 4,       // Vandalisme
    NaturalDisaster = 5, // Catastrophe naturelle
    GlassBreakage = 6,   // Bris de glace
    Other = 99           // Autre
}

/// <summary>
/// Sinistre automobile - hérite de Claim
/// </summary>
public class MotorClaim : Claim
{
    /// <summary>
    /// ID de la police automobile concernée
    /// </summary>
    public Guid MotorPolicyId { get; set; }
    public virtual MotorPolicy MotorPolicy { get; set; } = null!;

    /// <summary>
    /// Type de sinistre automobile
    /// </summary>
    public ClaimType ClaimType { get; set; }

    /// <summary>
    /// Y a-t-il des blessés ?
    /// </summary>
    public bool HasInjuries { get; set; }

    /// <summary>
    /// Nombre de blessés
    /// </summary>
    public int InjuryCount { get; set; }

    /// <summary>
    /// Y a-t-il eu un constat amiable ?
    /// </summary>
    public bool HasPoliceReport { get; set; }

    /// <summary>
    /// Numéro du constat/PV de police
    /// </summary>
    public string? PoliceReportNumber { get; set; }

    /// <summary>
    /// Nom du commissariat/gendarmerie
    /// </summary>
    public string? PoliceStation { get; set; }

    /// <summary>
    /// Tiers impliqués
    /// </summary>
    public virtual ICollection<ClaimThirdParty> ThirdParties { get; set; } = new List<ClaimThirdParty>();
}

/// <summary>
/// Tiers impliqué dans un sinistre automobile
/// </summary>
public class ClaimThirdParty : BaseEntity
{
    public Guid MotorClaimId { get; set; }
    public virtual MotorClaim MotorClaim { get; set; } = null!;

    /// <summary>
    /// Nom complet du tiers
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Téléphone
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Adresse
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Immatriculation du véhicule du tiers
    /// </summary>
    public string? VehicleRegistration { get; set; }

    /// <summary>
    /// Assureur du tiers
    /// </summary>
    public string? InsuranceCompany { get; set; }

    /// <summary>
    /// Numéro de police du tiers
    /// </summary>
    public string? PolicyNumber { get; set; }

    /// <summary>
    /// Est responsable ?
    /// </summary>
    public bool IsAtFault { get; set; }

    /// <summary>
    /// Pourcentage de responsabilité
    /// </summary>
    public decimal? FaultPercentage { get; set; }

    /// <summary>
    /// Description des dommages causés au tiers
    /// </summary>
    public string? DamageDescription { get; set; }
}
