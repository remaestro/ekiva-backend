using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class MotorCoverage : BaseEntity
{
    public string CoverageCode { get; set; } = string.Empty; // "SECTION_A", "SECTION_B", etc.
    public string CoverageName { get; set; } = string.Empty; // "Responsabilité Civile", "Défense et Recours"
    public string Description { get; set; } = string.Empty;
    public string SectionLetter { get; set; } = string.Empty; // "A", "B", "C", etc.
    public decimal FixedPremium { get; set; } // Prime fixe pour cette garantie
    public bool IsMandatory { get; set; } // Obligatoire ou optionnelle
    public bool IsActive { get; set; } = true;
    
    // Relations
    public ICollection<MotorProductCoverage> ProductCoverages { get; set; } = new List<MotorProductCoverage>();
}
