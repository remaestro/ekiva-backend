using System;
using System.Collections.Generic;
using Ekiva.Core.Common;

namespace Ekiva.Core.Entities
{
    public class Policy : BaseEntity
    {
        // Id is inherited from BaseEntity
        public string PolicyNumber { get; set; } = string.Empty; // Généré à la validation
        public string? QuotationNumber { get; set; } // Référence devis
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime IssueDate { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public string ProductCode { get; set; } = "MOTOR"; // Pour l'instant hardcodé

        public string Status { get; set; } = "Draft"; // Draft, Active, Cancelled, Expired

        // Financier
        public decimal TotalNetPremium { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal TotalGrossPremium { get; set; }
        public decimal CommissionAmount { get; set; }

        public Guid CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;

        // Relations
        public ICollection<PolicyRisk> Risks { get; set; } = new List<PolicyRisk>();
    }
}
