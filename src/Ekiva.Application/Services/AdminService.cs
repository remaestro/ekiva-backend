using Ekiva.Application.DTOs;
using Ekiva.Application.Interfaces;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;

namespace Ekiva.Application.Services;

/// <summary>
/// Service de gestion de l'administration et des statistiques
/// </summary>
public class AdminService : IAdminService
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Distributor> _distributorRepository;
    private readonly IRepository<MotorClaim> _claimRepository;
    private readonly IRepository<ASACICertificate> _certificateRepository;

    public AdminService(
        IRepository<Policy> policyRepository,
        IRepository<Client> clientRepository,
        IRepository<Distributor> distributorRepository,
        IRepository<MotorClaim> claimRepository,
        IRepository<ASACICertificate> certificateRepository)
    {
        _policyRepository = policyRepository;
        _clientRepository = clientRepository;
        _distributorRepository = distributorRepository;
        _claimRepository = claimRepository;
        _certificateRepository = certificateRepository;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardStats
        {
            Policies = await GetPolicyStatsAsync(),
            Clients = await GetClientStatsAsync(),
            Revenue = await GetRevenueStatsAsync(),
            Distributors = await GetDistributorStatsAsync(),
            Claims = await GetClaimStatsAsync(),
            Certificates = await GetCertificateStatsAsync()
        };

        return stats;
    }

    public async Task<PolicyStats> GetPolicyStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var policies = await _policyRepository.GetAllAsync();

        // Filter by date range if provided
        if (startDate.HasValue)
            policies = policies.Where(p => p.StartDate >= startDate.Value).ToList();
        if (endDate.HasValue)
            policies = policies.Where(p => p.StartDate <= endDate.Value).ToList();

        var stats = new PolicyStats
        {
            TotalPolicies = policies.Count,
            ActivePolicies = policies.Count(p => p.Status == "Active"),
            PendingPolicies = policies.Count(p => p.Status == "Draft"),
            ExpiredPolicies = policies.Count(p => p.Status == "Expired"),
            CancelledPolicies = policies.Count(p => p.Status == "Cancelled"),
            GrowthRate = CalculateGrowthRate(policies),
            ByProduct = policies.GroupBy(p => p.ProductCode)
                .Select(g => new PolicyByProduct
                {
                    ProductType = g.Key,
                    Count = g.Count(),
                    TotalPremium = g.Sum(p => p.TotalGrossPremium)
                }).ToList(),
            ByMonth = policies.GroupBy(p => p.StartDate.ToString("yyyy-MM"))
                .Select(g => new PolicyByMonth
                {
                    Month = g.Key,
                    Count = g.Count(),
                    TotalPremium = g.Sum(p => p.TotalGrossPremium)
                }).ToList()
        };

        return stats;
    }

    public async Task<ClientStats> GetClientStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var clients = await _clientRepository.GetAllAsync();

        // Filter by date range if provided
        if (startDate.HasValue)
            clients = clients.Where(c => c.CreatedAt >= startDate.Value).ToList();
        if (endDate.HasValue)
            clients = clients.Where(c => c.CreatedAt <= endDate.Value).ToList();

        var stats = new ClientStats
        {
            TotalClients = clients.Count,
            IndividualClients = clients.Count(c => c.Type == ClientType.Individual),
            CorporateClients = clients.Count(c => c.Type == ClientType.Company),
            NewClientsThisMonth = clients.Count(c => c.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            GrowthRate = CalculateClientGrowthRate(clients),
            ByMonth = clients.GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new ClientByMonth
                {
                    Month = g.Key,
                    Count = g.Count()
                }).ToList()
        };

        return stats;
    }

    public async Task<RevenueStats> GetRevenueStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var policies = await _policyRepository.GetAllAsync();

        // Filter by date range if provided
        if (startDate.HasValue)
            policies = policies.Where(p => p.StartDate >= startDate.Value).ToList();
        if (endDate.HasValue)
            policies = policies.Where(p => p.StartDate <= endDate.Value).ToList();

        var totalRevenue = policies.Sum(p => p.TotalGrossPremium);
        var currentMonth = DateTime.UtcNow;
        var lastMonth = currentMonth.AddMonths(-1);

        var stats = new RevenueStats
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = policies.Where(p => p.StartDate.Year == currentMonth.Year && p.StartDate.Month == currentMonth.Month)
                .Sum(p => p.TotalGrossPremium),
            RevenueLastMonth = policies.Where(p => p.StartDate.Year == lastMonth.Year && p.StartDate.Month == lastMonth.Month)
                .Sum(p => p.TotalGrossPremium),
            GrowthRate = CalculateRevenueGrowthRate(policies),
            TotalCommissions = policies.Sum(p => p.CommissionAmount),
            TotalTaxes = policies.Sum(p => p.TotalTaxes),
            NetRevenue = policies.Sum(p => p.TotalNetPremium),
            ByMonth = policies.GroupBy(p => p.StartDate.ToString("yyyy-MM"))
                .Select(g => new RevenueByMonth
                {
                    Month = g.Key,
                    GrossPremium = g.Sum(p => p.TotalGrossPremium),
                    NetPremium = g.Sum(p => p.TotalNetPremium),
                    Commissions = g.Sum(p => p.CommissionAmount),
                    Taxes = g.Sum(p => p.TotalTaxes)
                }).ToList(),
            ByProduct = policies.GroupBy(p => p.ProductCode)
                .Select(g => new RevenueByProduct
                {
                    ProductType = g.Key,
                    Amount = g.Sum(p => p.TotalGrossPremium),
                    Percentage = totalRevenue > 0 ? (g.Sum(p => p.TotalGrossPremium) / totalRevenue) * 100 : 0
                }).ToList()
        };

        return stats;
    }

    public async Task<DistributorStats> GetDistributorStatsAsync()
    {
        var distributors = await _distributorRepository.GetAllAsync();

        var stats = new DistributorStats
        {
            TotalDistributors = distributors.Count,
            ActiveDistributors = distributors.Count(d => d.IsActive),
            InternalAgents = distributors.Count(d => d.Type == DistributorType.InternalAgent),
            Brokers = distributors.Count(d => d.Type == DistributorType.Broker),
            GeneralAgents = distributors.Count(d => d.Type == DistributorType.GeneralAgent),
            TopDistributors = distributors.Where(d => d.IsActive)
                .Select(d => new TopDistributor
                {
                    DistributorId = d.Id,
                    Name = d.Name,
                    Type = d.Type.ToString(),
                    PoliciesCount = 0, // TODO: Link policies to distributors
                    TotalPremium = 0,
                    TotalCommission = 0
                })
                .OrderByDescending(d => d.TotalPremium)
                .Take(10)
                .ToList()
        };

        return stats;
    }

    public async Task<ClaimStats> GetClaimStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var claims = await _claimRepository.GetAllAsync();

        // Filter by date range if provided
        if (startDate.HasValue)
            claims = claims.Where(c => c.ClaimDate >= startDate.Value).ToList();
        if (endDate.HasValue)
            claims = claims.Where(c => c.ClaimDate <= endDate.Value).ToList();

        var stats = new ClaimStats
        {
            TotalClaims = claims.Count,
            PendingClaims = claims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview),
            ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
            RejectedClaims = claims.Count(c => c.Status == ClaimStatus.Rejected),
            TotalClaimAmount = claims.Sum(c => c.ClaimedAmount),
            PaidClaimAmount = claims.Where(c => c.Status == ClaimStatus.Settled).Sum(c => c.NetPayableAmount ?? 0),
            AverageClaimAmount = claims.Any() ? claims.Average(c => c.ClaimedAmount) : 0,
            ByMonth = claims.GroupBy(c => c.ClaimDate.ToString("yyyy-MM"))
                .Select(g => new ClaimByMonth
                {
                    Month = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(c => c.ClaimedAmount)
                }).ToList()
        };

        return stats;
    }

    private async Task<CertificateStats> GetCertificateStatsAsync()
    {
        var certificates = await _certificateRepository.GetAllAsync();

        var stats = new CertificateStats
        {
            TotalCertificates = certificates.Count,
            ActiveCertificates = certificates.Count(c => c.Status == CertificateStatus.Active && c.ExpiryDate > DateTime.UtcNow),
            SuspendedCertificates = certificates.Count(c => c.Status == CertificateStatus.Suspended),
            CancelledCertificates = certificates.Count(c => c.Status == CertificateStatus.Cancelled),
            ExpiredCertificates = certificates.Count(c => c.Status == CertificateStatus.Expired || (c.Status == CertificateStatus.Active && c.ExpiryDate <= DateTime.UtcNow)),
            ExpiringIn30Days = certificates.Count(c => c.Status == CertificateStatus.Active && 
                c.ExpiryDate > DateTime.UtcNow && 
                c.ExpiryDate <= DateTime.UtcNow.AddDays(30))
        };

        return stats;
    }

    public async Task<ReportResponse> GenerateReportAsync(ReportRequest request)
    {
        var response = new ReportResponse
        {
            ReportType = request.ReportType,
            GeneratedAt = DateTime.UtcNow,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        switch (request.ReportType.ToLower())
        {
            case "sales":
            case "policies":
                response.Data = await GetPolicyStatsAsync(request.StartDate, request.EndDate);
                break;
            case "revenue":
                response.Data = await GetRevenueStatsAsync(request.StartDate, request.EndDate);
                break;
            case "claims":
                response.Data = await GetClaimStatsAsync(request.StartDate, request.EndDate);
                break;
            case "clients":
                response.Data = await GetClientStatsAsync(request.StartDate, request.EndDate);
                break;
            default:
                response.Data = await GetDashboardStatsAsync();
                break;
        }

        return response;
    }

    public async Task<List<ActivityLog>> GetActivityLogsAsync(int page = 1, int pageSize = 50)
    {
        // TODO: Implement activity logging
        return new List<ActivityLog>();
    }

    public async Task<List<UserActivityResponse>> GetUserActivitiesAsync()
    {
        // TODO: Implement user activity tracking
        return new List<UserActivityResponse>();
    }

    public async Task<SystemConfiguration> GetSystemConfigurationAsync()
    {
        // TODO: Implement system configuration storage
        return new SystemConfiguration
        {
            CompanyName = "EKIVA Insurance",
            Currency = "XOF",
            TimeZone = "Africa/Porto-Novo",
            DateFormat = "dd/MM/yyyy"
        };
    }

    public async Task<SystemConfiguration> UpdateSystemConfigurationAsync(SystemConfiguration config)
    {
        // TODO: Implement system configuration update
        return config;
    }

    // Helper methods
    private decimal CalculateGrowthRate(IReadOnlyList<Policy> policies)
    {
        var currentMonth = DateTime.UtcNow;
        var lastMonth = currentMonth.AddMonths(-1);
        
        var currentMonthCount = policies.Count(p => p.StartDate.Year == currentMonth.Year && p.StartDate.Month == currentMonth.Month);
        var lastMonthCount = policies.Count(p => p.StartDate.Year == lastMonth.Year && p.StartDate.Month == lastMonth.Month);

        if (lastMonthCount == 0) return 0;
        return ((decimal)(currentMonthCount - lastMonthCount) / lastMonthCount) * 100;
    }

    private decimal CalculateClientGrowthRate(IReadOnlyList<Client> clients)
    {
        var currentMonth = DateTime.UtcNow;
        var lastMonth = currentMonth.AddMonths(-1);
        
        var currentMonthCount = clients.Count(c => c.CreatedAt.Year == currentMonth.Year && c.CreatedAt.Month == currentMonth.Month);
        var lastMonthCount = clients.Count(c => c.CreatedAt.Year == lastMonth.Year && c.CreatedAt.Month == lastMonth.Month);

        if (lastMonthCount == 0) return 0;
        return ((decimal)(currentMonthCount - lastMonthCount) / lastMonthCount) * 100;
    }

    private decimal CalculateRevenueGrowthRate(IReadOnlyList<Policy> policies)
    {
        var currentMonth = DateTime.UtcNow;
        var lastMonth = currentMonth.AddMonths(-1);
        
        var currentMonthRevenue = policies.Where(p => p.StartDate.Year == currentMonth.Year && p.StartDate.Month == currentMonth.Month)
            .Sum(p => p.TotalGrossPremium);
        var lastMonthRevenue = policies.Where(p => p.StartDate.Year == lastMonth.Year && p.StartDate.Month == lastMonth.Month)
            .Sum(p => p.TotalGrossPremium);

        if (lastMonthRevenue == 0) return 0;
        return ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
    }
}
