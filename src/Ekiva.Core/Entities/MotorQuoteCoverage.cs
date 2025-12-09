using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class MotorQuoteCoverage : BaseEntity
{
    public Guid MotorQuoteId { get; set; }
    public MotorQuote MotorQuote { get; set; } = null!;
    
    public Guid MotorCoverageId { get; set; }
    public MotorCoverage MotorCoverage { get; set; } = null!;
    
    public bool IsSelected { get; set; }
    public decimal PremiumAmount { get; set; } // Prime de cette garantie
    public string? CoverageLimit { get; set; } // Limite de garantie si applicable
    public string? Deductible { get; set; } // Franchise si applicable
}
