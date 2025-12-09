using System;
using System.Collections.Generic;

namespace Ekiva.Application.DTOs.Motor
{
    public class QuotationResponseDto
    {
        public Guid PolicyId { get; set; } // ID temporaire ou réel (Draft)
        public string QuotationNumber { get; set; } = string.Empty;
        public decimal TotalPremium { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public List<RiskResponseDto> Risks { get; set; } = new();
    }

    public class RiskResponseDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal NetPremium { get; set; }
        public List<CoverResponseDto> Covers { get; set; } = new();
    }

    public class CoverResponseDto
    {
        public string CoverCode { get; set; } = string.Empty;
        public string CoverName { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
        public decimal SumInsured { get; set; }
    }
}
