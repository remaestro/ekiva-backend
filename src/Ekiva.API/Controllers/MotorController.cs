using System;
using System.Threading.Tasks;
using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Interfaces;
using Ekiva.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotorController : ControllerBase
{
    private readonly IMotorQuoteService _motorQuoteService;
    private readonly IMotorPremiumCalculationService _premiumCalculationService;
    private readonly IMotorPolicyService _motorPolicyService;

    public MotorController(
        IMotorQuoteService motorQuoteService,
        IMotorPremiumCalculationService premiumCalculationService,
        IMotorPolicyService motorPolicyService)
    {
        _motorQuoteService = motorQuoteService;
        _premiumCalculationService = premiumCalculationService;
        _motorPolicyService = motorPolicyService;
    }

    #region Quotes Endpoints

    /// <summary>
    /// Créer un devis Motor Insurance
    /// </summary>
    [HttpPost("quotes")]
    public async Task<IActionResult> CreateQuote([FromBody] CreateMotorQuoteDto request)
    {
        try
        {
            var result = await _motorQuoteService.CreateQuoteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir un devis par ID
    /// </summary>
    [HttpGet("quotes/{id}")]
    public async Task<IActionResult> GetQuote(Guid id)
    {
        try
        {
            var quote = await _motorQuoteService.GetQuoteByIdAsync(id);
            return Ok(quote);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir tous les devis d'un client
    /// </summary>
    [HttpGet("quotes/client/{clientId}")]
    public async Task<IActionResult> GetQuotesByClient(Guid clientId)
    {
        try
        {
            var quotes = await _motorQuoteService.GetQuotesByClientIdAsync(clientId);
            return Ok(quotes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Accepter un devis
    /// </summary>
    [HttpPost("quotes/{id}/accept")]
    public async Task<IActionResult> AcceptQuote(Guid id)
    {
        try
        {
            var quote = await _motorQuoteService.AcceptQuoteAsync(id);
            return Ok(quote);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rejeter un devis
    /// </summary>
    [HttpPost("quotes/{id}/reject")]
    public async Task<IActionResult> RejectQuote(Guid id)
    {
        try
        {
            var result = await _motorQuoteService.RejectQuoteAsync(id);
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Calculer une prime (sans créer de devis)
    /// </summary>
    [HttpPost("calculate-premium")]
    public async Task<IActionResult> CalculatePremium([FromBody] MotorPremiumCalculationRequest request)
    {
        try
        {
            var result = await _premiumCalculationService.CalculatePremiumAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Policies Endpoints

    /// <summary>
    /// Convertir un devis accepté en police
    /// </summary>
    [HttpPost("quotes/{quoteId}/convert-to-policy")]
    public async Task<IActionResult> ConvertQuoteToPolicy(Guid quoteId)
    {
        try
        {
            var policy = await _motorPolicyService.ConvertQuoteToPolicyAsync(quoteId);
            return Ok(policy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir une police par ID
    /// </summary>
    [HttpGet("policies/{policyId}")]
    public async Task<IActionResult> GetPolicy(Guid policyId)
    {
        try
        {
            var policy = await _motorPolicyService.GetPolicyByIdAsync(policyId);
            return Ok(policy);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir toutes les polices d'un client
    /// </summary>
    [HttpGet("policies/client/{clientId}")]
    public async Task<IActionResult> GetPoliciesByClient(Guid clientId)
    {
        try
        {
            var policies = await _motorPolicyService.GetPoliciesByClientIdAsync(clientId);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Activer une police (après paiement)
    /// </summary>
    [HttpPost("policies/{policyId}/activate")]
    public async Task<IActionResult> ActivatePolicy(Guid policyId, [FromBody] ActivatePolicyRequest request)
    {
        try
        {
            var policy = await _motorPolicyService.ActivatePolicyAsync(policyId, request.PaymentReference);
            return Ok(policy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Suspendre une police
    /// </summary>
    [HttpPost("policies/{policyId}/suspend")]
    public async Task<IActionResult> SuspendPolicy(Guid policyId, [FromBody] PolicyActionRequest request)
    {
        try
        {
            var policy = await _motorPolicyService.SuspendPolicyAsync(policyId, request.Reason);
            return Ok(policy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Annuler une police
    /// </summary>
    [HttpPost("policies/{policyId}/cancel")]
    public async Task<IActionResult> CancelPolicy(Guid policyId, [FromBody] PolicyActionRequest request)
    {
        try
        {
            var policy = await _motorPolicyService.CancelPolicyAsync(policyId, request.Reason);
            return Ok(policy);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Créer un avenant (endorsement) sur une police
    /// </summary>
    [HttpPost("policies/{policyId}/endorsements")]
    public async Task<IActionResult> CreateEndorsement(Guid policyId, [FromBody] CreateEndorsementDto request)
    {
        try
        {
            var endorsement = await _motorPolicyService.CreateEndorsementAsync(policyId, request);
            return Ok(endorsement);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir tous les avenants d'une police
    /// </summary>
    [HttpGet("policies/{policyId}/endorsements")]
    public async Task<IActionResult> GetPolicyEndorsements(Guid policyId)
    {
        try
        {
            var endorsements = await _motorPolicyService.GetPolicyEndorsementsAsync(policyId);
            return Ok(endorsements);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}

// Request DTOs
public class ActivatePolicyRequest
{
    public string PaymentReference { get; set; } = string.Empty;
}

public class PolicyActionRequest
{
    public string Reason { get; set; } = string.Empty;
}
