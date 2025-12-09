using Ekiva.Application.DTOs.ASACI;
using Ekiva.Application.Interfaces;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Application.Services;

/// <summary>
/// Service de gestion des attestations ASACI
/// </summary>
public class ASACIService : IASACIService
{
    private readonly IRepository<ASACICertificate> _certificateRepository;
    private readonly IRepository<Policy> _policyRepository;

    public ASACIService(
        IRepository<ASACICertificate> certificateRepository,
        IRepository<Policy> policyRepository)
    {
        _certificateRepository = certificateRepository;
        _policyRepository = policyRepository;
    }

    public async Task<CertificateResponse> CreateCertificateAsync(CreateCertificateRequest request)
    {
        // Vérifier que la police existe
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId);
        if (policy == null)
        {
            throw new ArgumentException($"Policy with ID {request.PolicyId} not found.");
        }

        // Générer le numéro d'attestation ASACI
        var certificateNumber = await GenerateCertificateNumberAsync();

        // Créer l'attestation
        var certificate = new ASACICertificate
        {
            CertificateNumber = certificateNumber,
            PolicyId = request.PolicyId,
            VehicleRegistration = request.VehicleRegistration,
            VehicleMake = request.VehicleMake,
            VehicleModel = request.VehicleModel,
            ChassisNumber = request.ChassisNumber,
            PolicyHolderName = request.PolicyHolderName,
            PolicyHolderAddress = request.PolicyHolderAddress,
            PolicyHolderPhone = request.PolicyHolderPhone,
            EffectiveDate = request.EffectiveDate,
            ExpiryDate = request.ExpiryDate,
            CoverageType = request.CoverageType,
            Status = CertificateStatus.Active,
            IssuedDate = DateTime.UtcNow,
            ProductType = "Motor"
        };

        await _certificateRepository.AddAsync(certificate);

        return MapToResponse(certificate);
    }

    public async Task<VerificationResult> VerifyCertificateAsync(VerifyCertificateRequest request)
    {
        var certificates = await _certificateRepository.GetAllAsync();
        var certificate = certificates.FirstOrDefault(c => c.CertificateNumber == request.CertificateNumber);

        if (certificate == null)
        {
            return new VerificationResult
            {
                IsValid = false,
                CertificateNumber = request.CertificateNumber,
                Status = "NotFound",
                StatusMessage = "Attestation non trouvée dans le système ASACI"
            };
        }

        // Vérifier l'immatriculation si fournie
        if (!string.IsNullOrEmpty(request.VehicleRegistration) && 
            certificate.VehicleRegistration != request.VehicleRegistration)
        {
            return new VerificationResult
            {
                IsValid = false,
                CertificateNumber = request.CertificateNumber,
                Status = "Mismatch",
                StatusMessage = "L'immatriculation ne correspond pas à l'attestation",
                Certificate = MapToResponse(certificate)
            };
        }

        // Vérifier si expirée
        if (certificate.IsExpired())
        {
            return new VerificationResult
            {
                IsValid = false,
                CertificateNumber = certificate.CertificateNumber,
                Status = "Expired",
                StatusMessage = $"Attestation expirée le {certificate.ExpiryDate:dd/MM/yyyy}",
                Certificate = MapToResponse(certificate)
            };
        }

        // Vérifier le statut
        if (certificate.Status != CertificateStatus.Active)
        {
            return new VerificationResult
            {
                IsValid = false,
                CertificateNumber = certificate.CertificateNumber,
                Status = certificate.Status.ToString(),
                StatusMessage = GetStatusMessage(certificate.Status, certificate.StatusChangeReason),
                Certificate = MapToResponse(certificate)
            };
        }

        // Attestation valide
        return new VerificationResult
        {
            IsValid = true,
            CertificateNumber = certificate.CertificateNumber,
            Status = "Active",
            StatusMessage = $"Attestation valide jusqu'au {certificate.ExpiryDate:dd/MM/yyyy}",
            Certificate = MapToResponse(certificate)
        };
    }

    public async Task<CertificateResponse?> GetCertificateByNumberAsync(string certificateNumber)
    {
        var certificates = await _certificateRepository.GetAllAsync();
        var certificate = certificates.FirstOrDefault(c => c.CertificateNumber == certificateNumber);
        
        return certificate != null ? MapToResponse(certificate) : null;
    }

    public async Task<List<CertificateResponse>> GetAllCertificatesAsync()
    {
        var certificates = await _certificateRepository.GetAllAsync();
        return certificates
            .OrderByDescending(c => c.IssuedDate)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<CertificateResponse>> GetCertificatesByPolicyIdAsync(Guid policyId)
    {
        var certificates = await _certificateRepository.GetAllAsync();
        return certificates
            .Where(c => c.PolicyId == policyId)
            .OrderByDescending(c => c.IssuedDate)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<CertificateResponse> SuspendCertificateAsync(string certificateNumber, string reason)
    {
        var certificate = await GetCertificateEntityAsync(certificateNumber);
        
        if (certificate.Status == CertificateStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot suspend a cancelled certificate.");
        }

        certificate.Status = CertificateStatus.Suspended;
        certificate.StatusChangedDate = DateTime.UtcNow;
        certificate.StatusChangeReason = reason;

        await _certificateRepository.UpdateAsync(certificate);

        return MapToResponse(certificate);
    }

    public async Task<CertificateResponse> CancelCertificateAsync(string certificateNumber, string reason)
    {
        var certificate = await GetCertificateEntityAsync(certificateNumber);

        certificate.Status = CertificateStatus.Cancelled;
        certificate.StatusChangedDate = DateTime.UtcNow;
        certificate.StatusChangeReason = reason;

        await _certificateRepository.UpdateAsync(certificate);

        return MapToResponse(certificate);
    }

    public async Task<CertificateResponse> ReactivateCertificateAsync(string certificateNumber)
    {
        var certificate = await GetCertificateEntityAsync(certificateNumber);

        if (certificate.Status == CertificateStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot reactivate a cancelled certificate.");
        }

        if (certificate.IsExpired())
        {
            throw new InvalidOperationException("Cannot reactivate an expired certificate.");
        }

        certificate.Status = CertificateStatus.Active;
        certificate.StatusChangedDate = DateTime.UtcNow;
        certificate.StatusChangeReason = "Reactivated";

        await _certificateRepository.UpdateAsync(certificate);

        return MapToResponse(certificate);
    }

    public async Task<CertificateStats> GetStatisticsAsync()
    {
        var certificates = await _certificateRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var stats = new CertificateStats
        {
            TotalCertificates = certificates.Count(),
            ActiveCertificates = certificates.Count(c => c.Status == CertificateStatus.Active && !c.IsExpired()),
            SuspendedCertificates = certificates.Count(c => c.Status == CertificateStatus.Suspended),
            CancelledCertificates = certificates.Count(c => c.Status == CertificateStatus.Cancelled),
            ExpiredCertificates = certificates.Count(c => c.IsExpired()),
            ExpiringIn30Days = certificates.Count(c => 
                c.Status == CertificateStatus.Active && 
                c.DaysUntilExpiry() > 0 && 
                c.DaysUntilExpiry() <= 30)
        };

        // Statistiques par mois (6 derniers mois)
        stats.ByMonth = certificates
            .Where(c => c.IssuedDate >= now.AddMonths(-6))
            .GroupBy(c => c.IssuedDate.ToString("yyyy-MM"))
            .Select(g => new CertificateByMonth
            {
                Month = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToList();

        return stats;
    }

    public async Task<List<CertificateResponse>> GetExpiringCertificatesAsync(int daysThreshold = 30)
    {
        var certificates = await _certificateRepository.GetAllAsync();
        
        return certificates
            .Where(c => 
                c.Status == CertificateStatus.Active && 
                c.DaysUntilExpiry() > 0 && 
                c.DaysUntilExpiry() <= daysThreshold)
            .OrderBy(c => c.ExpiryDate)
            .Select(MapToResponse)
            .ToList();
    }

    #region Private Methods

    /// <summary>
    /// Générer un numéro d'attestation unique
    /// Format: ASACI-YYYY-XXXXXXXX
    /// </summary>
    private async Task<string> GenerateCertificateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var certificates = await _certificateRepository.GetAllAsync();
        var yearCertificates = certificates.Count(c => c.IssuedDate.Year == year);
        var sequence = (yearCertificates + 1).ToString("D8");
        
        return $"ASACI-{year}-{sequence}";
    }

    /// <summary>
    /// Récupérer une attestation par son numéro
    /// </summary>
    private async Task<ASACICertificate> GetCertificateEntityAsync(string certificateNumber)
    {
        var certificates = await _certificateRepository.GetAllAsync();
        var certificate = certificates.FirstOrDefault(c => c.CertificateNumber == certificateNumber);
        
        if (certificate == null)
        {
            throw new ArgumentException($"Certificate {certificateNumber} not found.");
        }

        return certificate;
    }

    /// <summary>
    /// Mapper une entité vers un DTO
    /// </summary>
    private CertificateResponse MapToResponse(ASACICertificate certificate)
    {
        return new CertificateResponse
        {
            Id = certificate.Id,
            CertificateNumber = certificate.CertificateNumber,
            PolicyId = certificate.PolicyId,
            VehicleRegistration = certificate.VehicleRegistration,
            VehicleMake = certificate.VehicleMake,
            VehicleModel = certificate.VehicleModel,
            ChassisNumber = certificate.ChassisNumber,
            PolicyHolderName = certificate.PolicyHolderName,
            PolicyHolderAddress = certificate.PolicyHolderAddress,
            PolicyHolderPhone = certificate.PolicyHolderPhone,
            EffectiveDate = certificate.EffectiveDate,
            ExpiryDate = certificate.ExpiryDate,
            Status = certificate.Status.ToString(),
            IssuedDate = certificate.IssuedDate,
            InsuranceCompanyName = certificate.InsuranceCompanyName,
            CoverageType = certificate.CoverageType,
            IsValid = certificate.IsValid(),
            DaysUntilExpiry = certificate.DaysUntilExpiry()
        };
    }

    /// <summary>
    /// Obtenir le message de statut
    /// </summary>
    private string GetStatusMessage(CertificateStatus status, string? reason)
    {
        return status switch
        {
            CertificateStatus.Suspended => $"Attestation suspendue: {reason}",
            CertificateStatus.Cancelled => $"Attestation annulée: {reason}",
            CertificateStatus.Expired => "Attestation expirée",
            _ => "Statut inconnu"
        };
    }

    #endregion
}
