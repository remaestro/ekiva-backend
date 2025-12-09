using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IMotorClaimService _claimService;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(IMotorClaimService claimService, ILogger<ClaimsController> logger)
    {
        _claimService = claimService;
        _logger = logger;
    }

    /// <summary>
    /// Crée un nouveau sinistre
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClaimDto>> CreateClaim([FromBody] CreateClaimDto request)
    {
        try
        {
            var claim = await _claimService.CreateClaimAsync(request);
            return CreatedAtAction(nameof(GetClaimById), new { id = claim.Id }, claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du sinistre");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un sinistre par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimDto>> GetClaimById(Guid id)
    {
        try
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du sinistre {ClaimId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un sinistre par son numéro
    /// </summary>
    [HttpGet("number/{claimNumber}")]
    public async Task<ActionResult<ClaimDto>> GetClaimByNumber(string claimNumber)
    {
        try
        {
            var claim = await _claimService.GetClaimByNumberAsync(claimNumber);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du sinistre {ClaimNumber}", claimNumber);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère tous les sinistres
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ClaimDto>>> GetAllClaims()
    {
        try
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des sinistres");
            return StatusCode(500, new { message = "Erreur lors de la récupération des sinistres" });
        }
    }

    /// <summary>
    /// Récupère les sinistres d'une police
    /// </summary>
    [HttpGet("policy/{policyId}")]
    public async Task<ActionResult<List<ClaimDto>>> GetClaimsByPolicy(Guid policyId)
    {
        try
        {
            var claims = await _claimService.GetClaimsByPolicyIdAsync(policyId);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des sinistres de la police {PolicyId}", policyId);
            return StatusCode(500, new { message = "Erreur lors de la récupération des sinistres" });
        }
    }

    /// <summary>
    /// Récupère les sinistres d'un client
    /// </summary>
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<List<ClaimDto>>> GetClaimsByClient(Guid clientId)
    {
        try
        {
            var claims = await _claimService.GetClaimsByClientIdAsync(clientId);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des sinistres du client {ClientId}", clientId);
            return StatusCode(500, new { message = "Erreur lors de la récupération des sinistres" });
        }
    }

    /// <summary>
    /// Récupère les sinistres par statut
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<ClaimDto>>> GetClaimsByStatus(ClaimStatus status)
    {
        try
        {
            var claims = await _claimService.GetClaimsByStatusAsync(status);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des sinistres par statut {Status}", status);
            return StatusCode(500, new { message = "Erreur lors de la récupération des sinistres" });
        }
    }

    /// <summary>
    /// Met à jour un sinistre (brouillon uniquement)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClaimDto>> UpdateClaim(Guid id, [FromBody] CreateClaimDto request)
    {
        try
        {
            var claim = await _claimService.UpdateClaimAsync(id, request);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Soumet un sinistre pour examen
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ClaimDto>> SubmitClaim(Guid id)
    {
        try
        {
            var claim = await _claimService.SubmitClaimAsync(id);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la soumission du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Démarre l'examen d'un sinistre
    /// </summary>
    [HttpPost("{id}/start-review")]
    public async Task<ActionResult<ClaimDto>> StartReview(Guid id, [FromBody] string reviewerUserId)
    {
        try
        {
            var claim = await _claimService.StartReviewAsync(id, reviewerUserId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'examen du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assigne un expert à un sinistre
    /// </summary>
    [HttpPost("{id}/assign-expert")]
    public async Task<ActionResult<ClaimDto>> AssignExpert(Guid id, [FromBody] AssignExpertDto request)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.AssignExpertAsync(id, request, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'assignation de l'expert au sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Soumet le rapport d'expertise
    /// </summary>
    [HttpPost("{id}/submit-expertise")]
    public async Task<ActionResult<ClaimDto>> SubmitExpertise(Guid id, [FromBody] SubmitExpertiseDto request)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.SubmitExpertiseAsync(id, request, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la soumission de l'expertise du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Approuve un sinistre
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ClaimDto>> ApproveClaim(Guid id, [FromBody] ApproveClaimDto request)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.ApproveClaimAsync(id, request, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'approbation du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rejette un sinistre
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ClaimDto>> RejectClaim(Guid id, [FromBody] RejectClaimDto request)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.RejectClaimAsync(id, request, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rejet du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Règle un sinistre
    /// </summary>
    [HttpPost("{id}/settle")]
    public async Task<ActionResult<ClaimDto>> SettleClaim(Guid id, [FromBody] SettleClaimDto request)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.SettleClaimAsync(id, request, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du règlement du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clôture un sinistre
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<ActionResult<ClaimDto>> CloseClaim(Guid id)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.CloseClaimAsync(id, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la clôture du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ajoute un document à un sinistre
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<ActionResult<ClaimDocumentDto>> UploadDocument(
        Guid id, 
        [FromForm] IFormFile file,
        [FromForm] string documentType,
        [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Aucun fichier fourni" });

            // TODO: Implémenter le stockage du fichier (local, cloud, etc.)
            var fileName = file.FileName;
            var filePath = $"claims/{id}/{fileName}"; // Chemin simplifié
            var contentType = file.ContentType;
            var fileSize = file.Length;

            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";

            var document = await _claimService.UploadDocumentAsync(
                id, documentType, fileName, filePath, contentType, fileSize, userId, description);

            return CreatedAtAction(nameof(GetClaimDocuments), new { id }, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'upload du document pour le sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les documents d'un sinistre
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<List<ClaimDocumentDto>>> GetClaimDocuments(Guid id)
    {
        try
        {
            var documents = await _claimService.GetClaimDocumentsAsync(id);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des documents du sinistre {ClaimId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération des documents" });
        }
    }

    /// <summary>
    /// Supprime un document
    /// </summary>
    [HttpDelete("{id}/documents/{documentId}")]
    public async Task<ActionResult> DeleteDocument(Guid id, Guid documentId)
    {
        try
        {
            var result = await _claimService.DeleteDocumentAsync(documentId);
            if (!result)
                return NotFound(new { message = "Document non trouvé" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId}", documentId);
            return StatusCode(500, new { message = "Erreur lors de la suppression du document" });
        }
    }

    /// <summary>
    /// Ajoute un tiers à un sinistre
    /// </summary>
    [HttpPost("{id}/third-parties")]
    public async Task<ActionResult<ThirdPartyDto>> AddThirdParty(Guid id, [FromBody] CreateThirdPartyDto request)
    {
        try
        {
            var thirdParty = await _claimService.AddThirdPartyAsync(id, request);
            return CreatedAtAction(nameof(GetClaimById), new { id }, thirdParty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du tiers au sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Met à jour un tiers
    /// </summary>
    [HttpPut("{id}/third-parties/{thirdPartyId}")]
    public async Task<ActionResult<ThirdPartyDto>> UpdateThirdParty(
        Guid id, 
        Guid thirdPartyId, 
        [FromBody] CreateThirdPartyDto request)
    {
        try
        {
            var thirdParty = await _claimService.UpdateThirdPartyAsync(thirdPartyId, request);
            return Ok(thirdParty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du tiers {ThirdPartyId}", thirdPartyId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Supprime un tiers
    /// </summary>
    [HttpDelete("{id}/third-parties/{thirdPartyId}")]
    public async Task<ActionResult> RemoveThirdParty(Guid id, Guid thirdPartyId)
    {
        try
        {
            var result = await _claimService.RemoveThirdPartyAsync(thirdPartyId);
            if (!result)
                return NotFound(new { message = "Tiers non trouvé" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du tiers {ThirdPartyId}", thirdPartyId);
            return StatusCode(500, new { message = "Erreur lors de la suppression du tiers" });
        }
    }

    /// <summary>
    /// Récupère l'historique d'un sinistre
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<ClaimHistoryDto>>> GetClaimHistory(Guid id)
    {
        try
        {
            var history = await _claimService.GetClaimHistoryAsync(id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'historique du sinistre {ClaimId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'historique" });
        }
    }

    /// <summary>
    /// Met à jour les notes internes d'un sinistre
    /// </summary>
    [HttpPatch("{id}/notes")]
    public async Task<ActionResult<ClaimDto>> UpdateInternalNotes(Guid id, [FromBody] string notes)
    {
        try
        {
            // TODO: Récupérer l'ID utilisateur depuis le token d'authentification
            var userId = "current-user-id";
            var claim = await _claimService.UpdateInternalNotesAsync(id, notes, userId);
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des notes du sinistre {ClaimId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
