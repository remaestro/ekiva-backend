using Ekiva.Application.DTOs;

namespace Ekiva.Application.Interfaces;

/// <summary>
/// Interface pour le service Admin
/// Gère les statistiques, rapports et configuration système
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Obtenir les statistiques du dashboard
    /// </summary>
    Task<DashboardStats> GetDashboardStatsAsync();

    /// <summary>
    /// Obtenir les statistiques des polices
    /// </summary>
    Task<PolicyStats> GetPolicyStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Obtenir les statistiques des clients
    /// </summary>
    Task<ClientStats> GetClientStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Obtenir les statistiques de revenus
    /// </summary>
    Task<RevenueStats> GetRevenueStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Obtenir les statistiques des distributeurs
    /// </summary>
    Task<DistributorStats> GetDistributorStatsAsync();

    /// <summary>
    /// Obtenir les statistiques des sinistres
    /// </summary>
    Task<ClaimStats> GetClaimStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Générer un rapport
    /// </summary>
    Task<ReportResponse> GenerateReportAsync(ReportRequest request);

    /// <summary>
    /// Obtenir les logs d'activité
    /// </summary>
    Task<List<ActivityLog>> GetActivityLogsAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Obtenir l'activité des utilisateurs
    /// </summary>
    Task<List<UserActivityResponse>> GetUserActivitiesAsync();

    /// <summary>
    /// Obtenir la configuration système
    /// </summary>
    Task<SystemConfiguration> GetSystemConfigurationAsync();

    /// <summary>
    /// Mettre à jour la configuration système
    /// </summary>
    Task<SystemConfiguration> UpdateSystemConfigurationAsync(SystemConfiguration config);
}
