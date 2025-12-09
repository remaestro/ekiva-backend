using Ekiva.Application.DTOs.ASACI;
using Ekiva.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ASACIController : ControllerBase
{
    private readonly IASACIService _asaciService;

    public ASACIController(IASACIService asaciService)
    {
        _asaciService = asaciService;
    }

    /// <summary>
    /// Créer une nouvelle attestation ASACI
    /// </summary>
    [HttpPost("certificates")]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CertificateResponse>> CreateCertificate([FromBody] CreateCertificateRequest request)
    {
        try
        {
            var result = await _asaciService.CreateCertificateAsync(request);
            return CreatedAtAction(nameof(GetCertificateByNumber), new { certificateNumber = result.CertificateNumber }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Vérifier la validité d'une attestation
    /// </summary>
    [AllowAnonymous]
    [HttpPost("certificates/verify")]
    [ProducesResponseType(typeof(VerificationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationResult>> VerifyCertificate([FromBody] VerifyCertificateRequest request)
    {
        var result = await _asaciService.VerifyCertificateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Obtenir une attestation par son numéro
    /// </summary>
    [HttpGet("certificates/{certificateNumber}")]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateResponse>> GetCertificateByNumber(string certificateNumber)
    {
        var result = await _asaciService.GetCertificateByNumberAsync(certificateNumber);
        if (result == null)
        {
            return NotFound(new { message = $"Certificate {certificateNumber} not found" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Obtenir toutes les attestations
    /// </summary>
    [HttpGet("certificates")]
    [ProducesResponseType(typeof(List<CertificateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificateResponse>>> GetAllCertificates()
    {
        var result = await _asaciService.GetAllCertificatesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtenir toutes les attestations d'une police
    /// </summary>
    [HttpGet("certificates/policy/{policyId}")]
    [ProducesResponseType(typeof(List<CertificateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificateResponse>>> GetCertificatesByPolicy(Guid policyId)
    {
        var result = await _asaciService.GetCertificatesByPolicyIdAsync(policyId);
        return Ok(result);
    }

    /// <summary>
    /// Suspendre une attestation
    /// </summary>
    [HttpPut("certificates/{certificateNumber}/suspend")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CertificateResponse>> SuspendCertificate(
        string certificateNumber, 
        [FromBody] UpdateCertificateStatusRequest request)
    {
        try
        {
            var result = await _asaciService.SuspendCertificateAsync(certificateNumber, request.Reason);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Annuler une attestation
    /// </summary>
    [HttpPut("certificates/{certificateNumber}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CertificateResponse>> CancelCertificate(
        string certificateNumber, 
        [FromBody] UpdateCertificateStatusRequest request)
    {
        try
        {
            var result = await _asaciService.CancelCertificateAsync(certificateNumber, request.Reason);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Réactiver une attestation suspendue
    /// </summary>
    [HttpPut("certificates/{certificateNumber}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CertificateResponse>> ReactivateCertificate(string certificateNumber)
    {
        try
        {
            var result = await _asaciService.ReactivateCertificateAsync(certificateNumber);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir les statistiques des attestations
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CertificateStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<CertificateStats>> GetStatistics()
    {
        var result = await _asaciService.GetStatisticsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtenir les attestations qui expirent bientôt
    /// </summary>
    [HttpGet("certificates/expiring")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(List<CertificateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificateResponse>>> GetExpiringCertificates([FromQuery] int days = 30)
    {
        var result = await _asaciService.GetExpiringCertificatesAsync(days);
        return Ok(result);
    }
}
