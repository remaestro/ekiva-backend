using Ekiva.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IPdfGenerationService _pdfGenerationService;

    public DocumentsController(IPdfGenerationService pdfGenerationService)
    {
        _pdfGenerationService = pdfGenerationService;
    }

    /// <summary>
    /// Télécharger le PDF d'un devis
    /// </summary>
    [HttpGet("quotes/{quoteId}/pdf")]
    public async Task<IActionResult> DownloadQuotePdf(Guid quoteId)
    {
        try
        {
            var pdfBytes = await _pdfGenerationService.GenerateQuotePdfAsync(quoteId);
            return File(pdfBytes, "application/pdf", $"Devis-{quoteId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Télécharger le PDF d'une police
    /// </summary>
    [HttpGet("policies/{policyId}/pdf")]
    public async Task<IActionResult> DownloadPolicyPdf(Guid policyId)
    {
        try
        {
            var pdfBytes = await _pdfGenerationService.GeneratePolicyPdfAsync(policyId);
            return File(pdfBytes, "application/pdf", $"Police-{policyId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Télécharger l'attestation d'assurance
    /// </summary>
    [HttpGet("policies/{policyId}/certificate")]
    public async Task<IActionResult> DownloadInsuranceCertificate(Guid policyId)
    {
        try
        {
            var pdfBytes = await _pdfGenerationService.GenerateInsuranceCertificateAsync(policyId);
            return File(pdfBytes, "application/pdf", $"Attestation-{policyId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Télécharger le PDF d'un avenant
    /// </summary>
    [HttpGet("endorsements/{endorsementId}/pdf")]
    public async Task<IActionResult> DownloadEndorsementPdf(Guid endorsementId)
    {
        try
        {
            var pdfBytes = await _pdfGenerationService.GenerateEndorsementPdfAsync(endorsementId);
            return File(pdfBytes, "application/pdf", $"Avenant-{endorsementId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
