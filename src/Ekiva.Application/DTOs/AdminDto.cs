namespace Ekiva.Application.DTOs;

/// <summary>
/// Dashboard statistics overview
/// </summary>
public class DashboardStats
{
    public PolicyStats Policies { get; set; } = new();
    public ClientStats Clients { get; set; } = new();
    public RevenueStats Revenue { get; set; } = new();
    public DistributorStats Distributors { get; set; } = new();
    public ClaimStats Claims { get; set; } = new();
    public CertificateStats Certificates { get; set; } = new();
}

/// <summary>
/// Policy statistics
/// </summary>
public class PolicyStats
{
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int PendingPolicies { get; set; }
    public int ExpiredPolicies { get; set; }
    public int CancelledPolicies { get; set; }
    public decimal GrowthRate { get; set; }
    public List<PolicyByProduct> ByProduct { get; set; } = new();
    public List<PolicyByMonth> ByMonth { get; set; } = new();
}

public class PolicyByProduct
{
    public string ProductType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalPremium { get; set; }
}

public class PolicyByMonth
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalPremium { get; set; }
}

/// <summary>
/// Client statistics
/// </summary>
public class ClientStats
{
    public int TotalClients { get; set; }
    public int IndividualClients { get; set; }
    public int CorporateClients { get; set; }
    public int NewClientsThisMonth { get; set; }
    public decimal GrowthRate { get; set; }
    public List<ClientByMonth> ByMonth { get; set; } = new();
}

public class ClientByMonth
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Revenue statistics
/// </summary>
public class RevenueStats
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal GrowthRate { get; set; }
    public decimal TotalCommissions { get; set; }
    public decimal TotalTaxes { get; set; }
    public decimal NetRevenue { get; set; }
    public List<RevenueByMonth> ByMonth { get; set; } = new();
    public List<RevenueByProduct> ByProduct { get; set; } = new();
}

public class RevenueByMonth
{
    public string Month { get; set; } = string.Empty;
    public decimal GrossPremium { get; set; }
    public decimal NetPremium { get; set; }
    public decimal Commissions { get; set; }
    public decimal Taxes { get; set; }
}

public class RevenueByProduct
{
    public string ProductType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Distributor statistics
/// </summary>
public class DistributorStats
{
    public int TotalDistributors { get; set; }
    public int ActiveDistributors { get; set; }
    public int InternalAgents { get; set; }
    public int Brokers { get; set; }
    public int GeneralAgents { get; set; }
    public List<TopDistributor> TopDistributors { get; set; } = new();
}

public class TopDistributor
{
    public Guid DistributorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int PoliciesCount { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal TotalCommission { get; set; }
}

/// <summary>
/// Claim statistics
/// </summary>
public class ClaimStats
{
    public int TotalClaims { get; set; }
    public int PendingClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int RejectedClaims { get; set; }
    public decimal TotalClaimAmount { get; set; }
    public decimal PaidClaimAmount { get; set; }
    public decimal AverageClaimAmount { get; set; }
    public List<ClaimByMonth> ByMonth { get; set; } = new();
}

public class ClaimByMonth
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Certificate statistics (ASACI)
/// </summary>
public class CertificateStats
{
    public int TotalCertificates { get; set; }
    public int ActiveCertificates { get; set; }
    public int SuspendedCertificates { get; set; }
    public int CancelledCertificates { get; set; }
    public int ExpiredCertificates { get; set; }
    public int ExpiringIn30Days { get; set; }
}

/// <summary>
/// Activity log entry
/// </summary>
public class ActivityLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// System configuration
/// </summary>
public class SystemConfiguration
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string CompanyWebsite { get; set; } = string.Empty;
    public string CompanyLogo { get; set; } = string.Empty;
    public string Currency { get; set; } = "XOF";
    public string TimeZone { get; set; } = "Africa/Porto-Novo";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public int PolicyNumberPrefix { get; set; } = 1;
    public int CertificateNumberPrefix { get; set; } = 1;
}

/// <summary>
/// Report request
/// </summary>
public class ReportRequest
{
    public string ReportType { get; set; } = string.Empty; // Sales, Claims, Commissions, etc.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ProductType { get; set; }
    public Guid? DistributorId { get; set; }
    public string? GroupBy { get; set; } // Day, Week, Month, Year
    public string Format { get; set; } = "JSON"; // JSON, PDF, Excel
}

/// <summary>
/// Report response
/// </summary>
public class ReportResponse
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public object Data { get; set; } = new();
    public ReportSummary Summary { get; set; } = new();
}

public class ReportSummary
{
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public Dictionary<string, decimal> Aggregates { get; set; } = new();
}

/// <summary>
/// User activity response
/// </summary>
public class UserActivityResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
    public int TotalPoliciesCreated { get; set; }
    public int TotalQuotesCreated { get; set; }
    public int TotalClientsCreated { get; set; }
    public decimal TotalRevenue { get; set; }
}
