using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

/// <summary>
/// Statut d'un sinistre
/// </summary>
public enum ClaimStatus
{
    Draft = 0,           // Brouillon (en cours de saisie)
    Submitted = 1,       // Déclaré (soumis par le client)
    UnderReview = 2,     // En cours d'examen
    Investigating = 3,   // En cours d'enquête
    Approved = 4,        // Approuvé (indemnisation acceptée)
    Rejected = 5,        // Rejeté
    Settled = 6,         // Réglé (paiement effectué)
    Closed = 7           // Clôturé
}

/// <summary>
/// Classe de base pour tous les sinistres
/// </summary>
public abstract class Claim : BaseEntity
{
    /// <summary>
    /// Numéro unique du sinistre (format: SIN-2025-12-0001)
    /// </summary>
    public string ClaimNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID du client (assuré)
    /// </summary>
    public Guid ClientId { get; set; }
    public virtual Client Client { get; set; } = null!;

    /// <summary>
    /// Type de produit (Motor, Fire, Liability, etc.)
    /// </summary>
    public ProductType ProductType { get; set; }

    /// <summary>
    /// Date du sinistre
    /// </summary>
    public DateTime ClaimDate { get; set; }

    /// <summary>
    /// Date de déclaration
    /// </summary>
    public DateTime ReportedDate { get; set; }

    /// <summary>
    /// Lieu du sinistre
    /// </summary>
    public string ClaimLocation { get; set; } = string.Empty;

    /// <summary>
    /// Statut du sinistre
    /// </summary>
    public ClaimStatus Status { get; set; }

    /// <summary>
    /// Description détaillée du sinistre
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Circonstances du sinistre
    /// </summary>
    public string? Circumstances { get; set; }

    /// <summary>
    /// Montant réclamé par l'assuré
    /// </summary>
    public decimal ClaimedAmount { get; set; }

    /// <summary>
    /// Montant estimé par l'expert
    /// </summary>
    public decimal? EstimatedAmount { get; set; }

    /// <summary>
    /// Montant approuvé pour indemnisation
    /// </summary>
    public decimal? ApprovedAmount { get; set; }

    /// <summary>
    /// Franchise applicable
    /// </summary>
    public decimal? Deductible { get; set; }

    /// <summary>
    /// Montant net à payer (ApprovedAmount - Deductible)
    /// </summary>
    public decimal? NetPayableAmount { get; set; }

    /// <summary>
    /// Expert assigné
    /// </summary>
    public string? AssignedExpert { get; set; }

    /// <summary>
    /// Date de l'expertise
    /// </summary>
    public DateTime? ExpertiseDate { get; set; }

    /// <summary>
    /// Rapport d'expertise
    /// </summary>
    public string? ExpertiseReport { get; set; }

    /// <summary>
    /// Date d'approbation
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Approuvé par (user ID)
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date de règlement
    /// </summary>
    public DateTime? SettlementDate { get; set; }

    /// <summary>
    /// Référence de paiement
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Mode de paiement
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Date de clôture
    /// </summary>
    public DateTime? ClosedDate { get; set; }

    /// <summary>
    /// Motif de rejet (si rejeté)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Notes internes
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Documents attachés
    /// </summary>
    public virtual ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();

    /// <summary>
    /// Historique des actions
    /// </summary>
    public virtual ICollection<ClaimHistory> History { get; set; } = new List<ClaimHistory>();
}

/// <summary>
/// Document attaché à un sinistre
/// </summary>
public class ClaimDocument : BaseEntity
{
    public Guid ClaimId { get; set; }
    public virtual Claim Claim { get; set; } = null!;

    /// <summary>
    /// Type de document (Constat, Photos, Facture, etc.)
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Nom du fichier
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Chemin de stockage
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Type MIME
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Taille du fichier (bytes)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Date d'upload
    /// </summary>
    public DateTime UploadedDate { get; set; }

    /// <summary>
    /// Uploadé par (user ID)
    /// </summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Description du document
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Historique des actions sur un sinistre
/// </summary>
public class ClaimHistory : BaseEntity
{
    public Guid ClaimId { get; set; }
    public virtual Claim Claim { get; set; } = null!;

    /// <summary>
    /// Date de l'action
    /// </summary>
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// Type d'action
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Description de l'action
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ancien statut
    /// </summary>
    public ClaimStatus? OldStatus { get; set; }

    /// <summary>
    /// Nouveau statut
    /// </summary>
    public ClaimStatus? NewStatus { get; set; }

    /// <summary>
    /// Effectué par (user ID)
    /// </summary>
    public string? PerformedBy { get; set; }

    /// <summary>
    /// Commentaire
    /// </summary>
    public string? Comment { get; set; }
}
