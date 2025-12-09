using Ekiva.Core.Entities;

namespace Ekiva.Application.DTOs.ASACI;

/// <summary>
/// Requête de création d'une attestation ASACI
/// </summary>
public class CreateCertificateRequest
{
    public Guid PolicyId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string PolicyHolderName { get; set; } = string.Empty;
    public string PolicyHolderAddress { get; set; } = string.Empty;
    public string PolicyHolderPhone { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CoverageType { get; set; } = string.Empty;
}

/// <summary>
/// Réponse après création d'une attestation
/// </summary>
public class CertificateResponse
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid PolicyId { get; set; }
    public string VehicleRegistration { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string PolicyHolderName { get; set; } = string.Empty;
    public string PolicyHolderAddress { get; set; } = string.Empty;
    public string PolicyHolderPhone { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string InsuranceCompanyName { get; set; } = string.Empty;
    public string CoverageType { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int DaysUntilExpiry { get; set; }
}

/// <summary>
/// Requête de vérification d'une attestation
/// </summary>
public class VerifyCertificateRequest
{
    public string CertificateNumber { get; set; } = string.Empty;
    public string? VehicleRegistration { get; set; }
}

/// <summary>
/// Résultat de vérification d'une attestation
/// </summary>
public class VerificationResult
{
    public bool IsValid { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public CertificateResponse? Certificate { get; set; }
}

/// <summary>
/// Requête de changement de statut
/// </summary>
public class UpdateCertificateStatusRequest
{
    public string Status { get; set; } = string.Empty; // Active, Suspended, Cancelled
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Statistiques des attestations ASACI
/// </summary>
public class CertificateStats
{
    public int TotalCertificates { get; set; }
    public int ActiveCertificates { get; set; }
    public int SuspendedCertificates { get; set; }
    public int CancelledCertificates { get; set; }
    public int ExpiredCertificates { get; set; }
    public int ExpiringIn30Days { get; set; }
    public List<CertificateByMonth> ByMonth { get; set; } = new();
}

public class CertificateByMonth
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
