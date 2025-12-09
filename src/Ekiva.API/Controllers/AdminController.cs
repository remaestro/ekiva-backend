using Ekiva.Application.DTOs;
using Ekiva.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[Authorize(Roles = "Admin,Manager")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Obtenir toutes les statistiques du dashboard
    /// </summary>
    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(DashboardStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStats>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Obtenir les statistiques des polices
    /// </summary>
    [HttpGet("stats/policies")]
    [ProducesResponseType(typeof(PolicyStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<PolicyStats>> GetPolicyStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _adminService.GetPolicyStatsAsync(startDate, endDate);
        return Ok(stats);
    }

    /// <summary>
    /// Obtenir les statistiques des clients
    /// </summary>
    [HttpGet("stats/clients")]
    [ProducesResponseType(typeof(ClientStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClientStats>> GetClientStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _adminService.GetClientStatsAsync(startDate, endDate);
        return Ok(stats);
    }

    /// <summary>
    /// Obtenir les statistiques de revenus
    /// </summary>
    [HttpGet("stats/revenue")]
    [ProducesResponseType(typeof(RevenueStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevenueStats>> GetRevenueStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _adminService.GetRevenueStatsAsync(startDate, endDate);
        return Ok(stats);
    }

    /// <summary>
    /// Obtenir les statistiques des distributeurs
    /// </summary>
    [HttpGet("stats/distributors")]
    [ProducesResponseType(typeof(DistributorStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<DistributorStats>> GetDistributorStats()
    {
        var stats = await _adminService.GetDistributorStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Obtenir les statistiques des sinistres
    /// </summary>
    [HttpGet("stats/claims")]
    [ProducesResponseType(typeof(ClaimStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClaimStats>> GetClaimStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _adminService.GetClaimStatsAsync(startDate, endDate);
        return Ok(stats);
    }

    /// <summary>
    /// Générer un rapport
    /// </summary>
    [HttpPost("reports/generate")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportResponse>> GenerateReport([FromBody] ReportRequest request)
    {
        try
        {
            var report = await _adminService.GenerateReportAsync(request);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir les logs d'activité
    /// </summary>
    [HttpGet("activity-logs")]
    [ProducesResponseType(typeof(List<ActivityLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ActivityLog>>> GetActivityLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var logs = await _adminService.GetActivityLogsAsync(page, pageSize);
        return Ok(logs);
    }

    /// <summary>
    /// Obtenir l'activité des utilisateurs
    /// </summary>
    [HttpGet("user-activities")]
    [ProducesResponseType(typeof(List<UserActivityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserActivityResponse>>> GetUserActivities()
    {
        var activities = await _adminService.GetUserActivitiesAsync();
        return Ok(activities);
    }

    /// <summary>
    /// Obtenir la configuration système
    /// </summary>
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(SystemConfiguration), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemConfiguration>> GetSystemConfiguration()
    {
        var config = await _adminService.GetSystemConfigurationAsync();
        return Ok(config);
    }

    /// <summary>
    /// Mettre à jour la configuration système
    /// </summary>
    [HttpPut("configuration")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SystemConfiguration), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemConfiguration>> UpdateSystemConfiguration(
        [FromBody] SystemConfiguration config)
    {
        var updatedConfig = await _adminService.UpdateSystemConfigurationAsync(config);
        return Ok(updatedConfig);
    }
}
