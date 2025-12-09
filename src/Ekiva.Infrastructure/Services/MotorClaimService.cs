using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Services;

public class MotorClaimService : IMotorClaimService
{
    private readonly EkivaDbContext _context;

    public MotorClaimService(EkivaDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimDto> CreateClaimAsync(CreateClaimDto request)
    {
        // Vérifier que la police existe et est active
        var policy = await _context.MotorPolicies
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == request.MotorPolicyId);

        if (policy == null)
            throw new Exception("Police non trouvée");

        if (policy.Status != PolicyStatus.Active)
            throw new Exception("La police doit être active pour déclarer un sinistre");

        // Vérifier que le sinistre est dans la période de couverture
        if (request.ClaimDate < policy.PolicyStartDate || request.ClaimDate > policy.PolicyEndDate)
            throw new Exception("Le sinistre doit se produire pendant la période de couverture de la police");

        // Créer le sinistre
        var claim = new MotorClaim
        {
            Id = Guid.NewGuid(),
            ClaimNumber = await GenerateClaimNumberAsync(),
            MotorPolicyId = request.MotorPolicyId,
            ClientId = policy.ClientId,
            ClaimDate = request.ClaimDate,
            ReportedDate = DateTime.UtcNow,
            ClaimLocation = request.ClaimLocation,
            ClaimType = request.ClaimType,
            Status = ClaimStatus.Draft,
            Description = request.Description,
            Circumstances = request.Circumstances,
            HasInjuries = request.HasInjuries,
            InjuryCount = request.InjuryCount,
            HasPoliceReport = request.HasPoliceReport,
            PoliceReportNumber = request.PoliceReportNumber,
            PoliceStation = request.PoliceStation,
            ClaimedAmount = request.ClaimedAmount,
            CreatedAt = DateTime.UtcNow
        };

        _context.MotorClaims.Add(claim);

        // Ajouter les tiers si présents
        if (request.ThirdParties != null && request.ThirdParties.Any())
        {
            foreach (var thirdParty in request.ThirdParties)
            {
                var tp = new ClaimThirdParty
                {
                    Id = Guid.NewGuid(),
                    MotorClaimId = claim.Id,
                    FullName = thirdParty.FullName,
                    PhoneNumber = thirdParty.PhoneNumber,
                    Email = thirdParty.Email,
                    Address = thirdParty.Address,
                    VehicleRegistration = thirdParty.VehicleRegistration,
                    InsuranceCompany = thirdParty.InsuranceCompany,
                    PolicyNumber = thirdParty.PolicyNumber,
                    IsAtFault = thirdParty.IsAtFault,
                    FaultPercentage = thirdParty.FaultPercentage,
                    DamageDescription = thirdParty.DamageDescription,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ClaimThirdParties.Add(tp);
            }
        }

        // Ajouter l'entrée d'historique
        await AddHistoryEntryAsync(claim.Id, "ClaimCreated", "Sinistre créé", null, ClaimStatus.Draft, null);

        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claim.Id);
    }

    public async Task<ClaimDto> GetClaimByIdAsync(Guid claimId)
    {
        var claim = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null)
            throw new Exception($"Sinistre {claimId} non trouvé");

        return MapToDto(claim);
    }

    public async Task<ClaimDto> GetClaimByNumberAsync(string claimNumber)
    {
        var claim = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);

        if (claim == null)
            throw new Exception($"Sinistre {claimNumber} non trouvé");

        return MapToDto(claim);
    }

    public async Task<List<ClaimDto>> GetClaimsByPolicyIdAsync(Guid policyId)
    {
        var claims = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .Where(c => c.MotorPolicyId == policyId)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return claims.Select(MapToDto).ToList();
    }

    public async Task<List<ClaimDto>> GetClaimsByClientIdAsync(Guid clientId)
    {
        var claims = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return claims.Select(MapToDto).ToList();
    }

    public async Task<List<ClaimDto>> GetClaimsByStatusAsync(ClaimStatus status)
    {
        var claims = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return claims.Select(MapToDto).ToList();
    }

    public async Task<List<ClaimDto>> GetAllClaimsAsync()
    {
        var claims = await _context.MotorClaims
            .Include(c => c.MotorPolicy)
            .Include(c => c.Client)
            .Include(c => c.Documents)
            .Include(c => c.History)
            .Include(c => c.ThirdParties)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return claims.Select(MapToDto).ToList();
    }

    public async Task<ClaimDto> SubmitClaimAsync(Guid claimId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Draft)
            throw new Exception("Seul un sinistre en brouillon peut être soumis");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Submitted;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ClaimSubmitted", "Sinistre soumis pour examen", oldStatus, claim.Status, null);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> StartReviewAsync(Guid claimId, string reviewerUserId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Submitted)
            throw new Exception("Le sinistre doit être soumis pour commencer l'examen");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.UnderReview;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ReviewStarted", "Examen du sinistre commencé", 
            oldStatus, claim.Status, reviewerUserId);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> AssignExpertAsync(Guid claimId, AssignExpertDto request, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.UnderReview && claim.Status != ClaimStatus.Investigating)
            throw new Exception("Le sinistre doit être en cours d'examen pour assigner un expert");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Investigating;
        claim.AssignedExpert = request.ExpertName;
        claim.ExpertiseDate = request.ExpertiseDate;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ExpertAssigned", 
            $"Expert assigné: {request.ExpertName} - Date d'expertise: {request.ExpertiseDate:dd/MM/yyyy}", 
            oldStatus, claim.Status, userId, request.Notes);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> SubmitExpertiseAsync(Guid claimId, SubmitExpertiseDto request, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Investigating)
            throw new Exception("Le sinistre doit être en cours d'enquête");

        if (string.IsNullOrEmpty(claim.AssignedExpert))
            throw new Exception("Aucun expert n'est assigné à ce sinistre");

        claim.EstimatedAmount = request.EstimatedAmount;
        claim.ExpertiseReport = request.ExpertiseReport;
        if (request.RecommendedDeductible.HasValue)
            claim.Deductible = request.RecommendedDeductible.Value;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ExpertiseSubmitted", 
            $"Rapport d'expertise soumis - Montant estimé: {request.EstimatedAmount:N0} FCFA", 
            null, null, userId);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> ApproveClaimAsync(Guid claimId, ApproveClaimDto request, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Investigating && claim.Status != ClaimStatus.UnderReview)
            throw new Exception("Le sinistre doit être en cours d'examen ou d'enquête pour être approuvé");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Approved;
        claim.ApprovedAmount = request.ApprovedAmount;
        claim.Deductible = request.Deductible;
        claim.NetPayableAmount = request.ApprovedAmount - request.Deductible;
        claim.ApprovalDate = DateTime.UtcNow;
        claim.ApprovedBy = userId;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ClaimApproved", 
            $"Sinistre approuvé - Montant: {request.ApprovedAmount:N0} FCFA - Franchise: {request.Deductible:N0} FCFA - Net à payer: {claim.NetPayableAmount:N0} FCFA", 
            oldStatus, claim.Status, userId, request.Comments);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> RejectClaimAsync(Guid claimId, RejectClaimDto request, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status == ClaimStatus.Rejected || claim.Status == ClaimStatus.Settled || claim.Status == ClaimStatus.Closed)
            throw new Exception("Ce sinistre ne peut plus être rejeté");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Rejected;
        claim.RejectionReason = request.RejectionReason;
        claim.ClosedDate = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ClaimRejected", 
            $"Sinistre rejeté - Motif: {request.RejectionReason}", 
            oldStatus, claim.Status, userId, request.Comments);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> SettleClaimAsync(Guid claimId, SettleClaimDto request, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Approved)
            throw new Exception("Le sinistre doit être approuvé pour être réglé");

        if (!claim.NetPayableAmount.HasValue || claim.NetPayableAmount <= 0)
            throw new Exception("Le montant net à payer doit être supérieur à 0");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Settled;
        claim.SettlementDate = request.SettlementDate;
        claim.PaymentReference = request.PaymentReference;
        claim.PaymentMethod = request.PaymentMethod;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ClaimSettled", 
            $"Sinistre réglé - Montant payé: {claim.NetPayableAmount:N0} FCFA - Référence: {request.PaymentReference}", 
            oldStatus, claim.Status, userId, request.Comments);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> CloseClaimAsync(Guid claimId, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Settled && claim.Status != ClaimStatus.Rejected)
            throw new Exception("Le sinistre doit être réglé ou rejeté pour être clôturé");

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Closed;
        claim.ClosedDate = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "ClaimClosed", "Sinistre clôturé", 
            oldStatus, claim.Status, userId);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDocumentDto> UploadDocumentAsync(Guid claimId, string documentType, 
        string fileName, string filePath, string contentType, long fileSize, string userId, string? description = null)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        var document = new ClaimDocument
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = userId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClaimDocuments.Add(document);

        await AddHistoryEntryAsync(claimId, "DocumentUploaded", 
            $"Document ajouté: {documentType} - {fileName}", null, null, userId);
        await _context.SaveChangesAsync();

        return new ClaimDocumentDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FilePath = document.FilePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadedDate = document.UploadedDate,
            UploadedBy = document.UploadedBy,
            Description = document.Description
        };
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        var document = await _context.ClaimDocuments.FindAsync(documentId);
        if (document == null)
            return false;

        _context.ClaimDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId)
    {
        var documents = await _context.ClaimDocuments
            .Where(d => d.ClaimId == claimId)
            .OrderByDescending(d => d.UploadedDate)
            .ToListAsync();

        return documents.Select(d => new ClaimDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FilePath = d.FilePath,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            UploadedDate = d.UploadedDate,
            UploadedBy = d.UploadedBy,
            Description = d.Description
        }).ToList();
    }

    public async Task<ThirdPartyDto> AddThirdPartyAsync(Guid claimId, CreateThirdPartyDto request)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        var thirdParty = new ClaimThirdParty
        {
            Id = Guid.NewGuid(),
            MotorClaimId = claimId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Address = request.Address,
            VehicleRegistration = request.VehicleRegistration,
            InsuranceCompany = request.InsuranceCompany,
            PolicyNumber = request.PolicyNumber,
            IsAtFault = request.IsAtFault,
            FaultPercentage = request.FaultPercentage,
            DamageDescription = request.DamageDescription,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClaimThirdParties.Add(thirdParty);
        await _context.SaveChangesAsync();

        return MapThirdPartyToDto(thirdParty);
    }

    public async Task<bool> RemoveThirdPartyAsync(Guid thirdPartyId)
    {
        var thirdParty = await _context.ClaimThirdParties.FindAsync(thirdPartyId);
        if (thirdParty == null)
            return false;

        _context.ClaimThirdParties.Remove(thirdParty);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ThirdPartyDto> UpdateThirdPartyAsync(Guid thirdPartyId, CreateThirdPartyDto request)
    {
        var thirdParty = await _context.ClaimThirdParties.FindAsync(thirdPartyId);
        if (thirdParty == null)
            throw new Exception("Tiers non trouvé");

        thirdParty.FullName = request.FullName;
        thirdParty.PhoneNumber = request.PhoneNumber;
        thirdParty.Email = request.Email;
        thirdParty.Address = request.Address;
        thirdParty.VehicleRegistration = request.VehicleRegistration;
        thirdParty.InsuranceCompany = request.InsuranceCompany;
        thirdParty.PolicyNumber = request.PolicyNumber;
        thirdParty.IsAtFault = request.IsAtFault;
        thirdParty.FaultPercentage = request.FaultPercentage;
        thirdParty.DamageDescription = request.DamageDescription;
        thirdParty.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapThirdPartyToDto(thirdParty);
    }

    public async Task<List<ClaimHistoryDto>> GetClaimHistoryAsync(Guid claimId)
    {
        var history = await _context.ClaimHistories
            .Where(h => h.ClaimId == claimId)
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        return history.Select(h => new ClaimHistoryDto
        {
            Id = h.Id,
            ActionDate = h.ActionDate,
            ActionType = h.ActionType,
            Description = h.Description,
            OldStatus = h.OldStatus,
            NewStatus = h.NewStatus,
            PerformedBy = h.PerformedBy,
            Comment = h.Comment
        }).ToList();
    }

    public async Task<ClaimDto> UpdateInternalNotesAsync(Guid claimId, string notes, string userId)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        claim.InternalNotes = notes;
        claim.UpdatedAt = DateTime.UtcNow;

        await AddHistoryEntryAsync(claimId, "NotesUpdated", "Notes internes mises à jour", 
            null, null, userId);
        await _context.SaveChangesAsync();

        return await GetClaimByIdAsync(claimId);
    }

    public async Task<ClaimDto> UpdateClaimAsync(Guid claimId, CreateClaimDto request)
    {
        var claim = await _context.MotorClaims.FindAsync(claimId);
        if (claim == null)
            throw new Exception("Sinistre non trouvé");

        if (claim.Status != ClaimStatus.Draft)
            throw new Exception("Seul un sinistre en brouillon peut être modifié");

        claim.ClaimDate = request.ClaimDate;
        claim.ClaimLocation = request.ClaimLocation;
        claim.ClaimType = request.ClaimType;
        claim.Description = request.Description;
        claim.Circumstances = request.Circumstances;
        claim.HasInjuries = request.HasInjuries;
        claim.InjuryCount = request.InjuryCount;
        claim.HasPoliceReport = request.HasPoliceReport;
        claim.PoliceReportNumber = request.PoliceReportNumber;
        claim.PoliceStation = request.PoliceStation;
        claim.ClaimedAmount = request.ClaimedAmount;
        claim.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetClaimByIdAsync(claimId);
    }

    // Méthodes privées

    private async Task<string> GenerateClaimNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;

        var lastClaim = await _context.MotorClaims
            .Where(c => c.ClaimNumber.StartsWith($"SIN-{year}-{month:D2}"))
            .OrderByDescending(c => c.ClaimNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastClaim != null)
        {
            var parts = lastClaim.ClaimNumber.Split('-');
            if (parts.Length == 4 && int.TryParse(parts[3], out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"SIN-{year}-{month:D2}-{sequence:D4}";
    }

    private async Task AddHistoryEntryAsync(Guid claimId, string actionType, string description, 
        ClaimStatus? oldStatus, ClaimStatus? newStatus, string? userId, string? comment = null)
    {
        var history = new ClaimHistory
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            ActionDate = DateTime.UtcNow,
            ActionType = actionType,
            Description = description,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            PerformedBy = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClaimHistories.Add(history);
    }

    private ClaimDto MapToDto(MotorClaim claim)
    {
        return new ClaimDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            MotorPolicyId = claim.MotorPolicyId,
            PolicyNumber = claim.MotorPolicy.PolicyNumber,
            ClientId = claim.ClientId,
            ClientName = GetClientName(claim.Client),
            VehicleRegistration = claim.MotorPolicy.RegistrationNumber,
            ClaimDate = claim.ClaimDate,
            ReportedDate = claim.ReportedDate,
            ClaimLocation = claim.ClaimLocation,
            ClaimType = claim.ClaimType,
            ClaimTypeText = GetClaimTypeText(claim.ClaimType),
            Status = claim.Status,
            StatusText = GetStatusText(claim.Status),
            Description = claim.Description,
            Circumstances = claim.Circumstances,
            HasInjuries = claim.HasInjuries,
            InjuryCount = claim.InjuryCount,
            HasPoliceReport = claim.HasPoliceReport,
            PoliceReportNumber = claim.PoliceReportNumber,
            PoliceStation = claim.PoliceStation,
            ClaimedAmount = claim.ClaimedAmount,
            EstimatedAmount = claim.EstimatedAmount,
            ApprovedAmount = claim.ApprovedAmount,
            Deductible = claim.Deductible,
            NetPayableAmount = claim.NetPayableAmount,
            AssignedExpert = claim.AssignedExpert,
            ExpertiseDate = claim.ExpertiseDate,
            ExpertiseReport = claim.ExpertiseReport,
            ApprovalDate = claim.ApprovalDate,
            ApprovedBy = claim.ApprovedBy,
            SettlementDate = claim.SettlementDate,
            PaymentReference = claim.PaymentReference,
            PaymentMethod = claim.PaymentMethod,
            ClosedDate = claim.ClosedDate,
            RejectionReason = claim.RejectionReason,
            InternalNotes = claim.InternalNotes,
            Documents = claim.Documents.Select(d => new ClaimDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FilePath = d.FilePath,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                UploadedDate = d.UploadedDate,
                UploadedBy = d.UploadedBy,
                Description = d.Description
            }).ToList(),
            History = claim.History.OrderByDescending(h => h.ActionDate).Select(h => new ClaimHistoryDto
            {
                Id = h.Id,
                ActionDate = h.ActionDate,
                ActionType = h.ActionType,
                Description = h.Description,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                PerformedBy = h.PerformedBy,
                Comment = h.Comment
            }).ToList(),
            ThirdParties = claim.ThirdParties.Select(MapThirdPartyToDto).ToList(),
            CreatedAt = claim.CreatedAt
        };
    }

    private ThirdPartyDto MapThirdPartyToDto(ClaimThirdParty tp)
    {
        return new ThirdPartyDto
        {
            Id = tp.Id,
            FullName = tp.FullName,
            PhoneNumber = tp.PhoneNumber,
            Email = tp.Email,
            Address = tp.Address,
            VehicleRegistration = tp.VehicleRegistration,
            InsuranceCompany = tp.InsuranceCompany,
            PolicyNumber = tp.PolicyNumber,
            IsAtFault = tp.IsAtFault,
            FaultPercentage = tp.FaultPercentage,
            DamageDescription = tp.DamageDescription
        };
    }

    private string GetClientName(Client client)
    {
        return client.Type == ClientType.Individual
            ? $"{client.FirstName} {client.LastName}"
            : client.CompanyName ?? string.Empty;
    }

    private string GetClaimTypeText(ClaimType type)
    {
        return type switch
        {
            ClaimType.Accident => "Accident",
            ClaimType.Theft => "Vol",
            ClaimType.Fire => "Incendie",
            ClaimType.Vandalism => "Vandalisme",
            ClaimType.NaturalDisaster => "Catastrophe naturelle",
            ClaimType.GlassBreakage => "Bris de glace",
            ClaimType.Other => "Autre",
            _ => type.ToString()
        };
    }

    private string GetStatusText(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.Draft => "Brouillon",
            ClaimStatus.Submitted => "Soumis",
            ClaimStatus.UnderReview => "En cours d'examen",
            ClaimStatus.Investigating => "En enquête",
            ClaimStatus.Approved => "Approuvé",
            ClaimStatus.Rejected => "Rejeté",
            ClaimStatus.Settled => "Réglé",
            ClaimStatus.Closed => "Clôturé",
            _ => status.ToString()
        };
    }
}
