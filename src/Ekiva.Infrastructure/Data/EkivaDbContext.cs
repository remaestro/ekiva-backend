using Ekiva.Core.Common;
using Ekiva.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Data;

public class EkivaDbContext : IdentityDbContext<ApplicationUser>
{
    public EkivaDbContext(DbContextOptions<EkivaDbContext> options) : base(options)
    {
    }

    public DbSet<Currency> Currencies { get; set; }
    public DbSet<VehicleMake> VehicleMakes { get; set; }
    public DbSet<VehicleModel> VehicleModels { get; set; }
    public DbSet<VehicleCategory> VehicleCategories { get; set; }
    public DbSet<ProfessionalCategory> ProfessionalCategories { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Distributor> Distributors { get; set; }
    public DbSet<CommissionRate> CommissionRates { get; set; }
    public DbSet<ProductTaxRate> ProductTaxRates { get; set; }
    public DbSet<Subsidiary> Subsidiaries { get; set; }
    public DbSet<Branch> Branches { get; set; }

    // Motor / Policy
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyRisk> PolicyRisks { get; set; }
    public DbSet<PolicyCover> PolicyCovers { get; set; }
    
    // Motor Insurance Entities
    public DbSet<MotorProduct> MotorProducts { get; set; }
    public DbSet<MotorCoverage> MotorCoverages { get; set; }
    public DbSet<MotorProductCoverage> MotorProductCoverages { get; set; }
    public DbSet<RatingFactor> RatingFactors { get; set; }
    public DbSet<ShortTermFactor> ShortTermFactors { get; set; }
    public DbSet<PolicyCost> PolicyCosts { get; set; }
    public DbSet<MotorQuote> MotorQuotes { get; set; }
    public DbSet<MotorQuoteCoverage> MotorQuoteCoverages { get; set; }
    
    // Motor Policy Entities
    public DbSet<MotorPolicy> MotorPolicies { get; set; }
    public DbSet<MotorPolicyCoverage> MotorPolicyCoverages { get; set; }
    public DbSet<MotorPolicyEndorsement> MotorPolicyEndorsements { get; set; }
    
    // Motor Claims Entities
    public DbSet<MotorClaim> MotorClaims { get; set; }
    public DbSet<ClaimDocument> ClaimDocuments { get; set; }
    public DbSet<ClaimHistory> ClaimHistories { get; set; }
    public DbSet<ClaimThirdParty> ClaimThirdParties { get; set; }
    
    // ASACI Certificates
    public DbSet<ASACICertificate> ASACICertificates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(EkivaDbContext).Assembly);

        // Configuration ApplicationUser
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Branch)
            .WithMany()
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Subsidiary)
            .WithMany()
            .HasForeignKey(u => u.SubsidiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurations existantes...
        builder.Entity<Client>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Configuration Policy
        builder.Entity<Policy>()
            .HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Policy>()
            .Property(p => p.TotalNetPremium).HasPrecision(18, 2);
        builder.Entity<Policy>()
            .Property(p => p.TotalTaxes).HasPrecision(18, 2);
        builder.Entity<Policy>()
            .Property(p => p.TotalGrossPremium).HasPrecision(18, 2);
        builder.Entity<Policy>()
            .Property(p => p.CommissionAmount).HasPrecision(18, 2);

        // Configuration PolicyRisk
        builder.Entity<PolicyRisk>()
            .HasOne(r => r.Policy)
            .WithMany(p => p.Risks)
            .HasForeignKey(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PolicyRisk>()
            .Property(r => r.VehicleValue).HasPrecision(18, 2);
        builder.Entity<PolicyRisk>()
            .Property(r => r.NetPremium).HasPrecision(18, 2);

        // Configuration PolicyCover
        builder.Entity<PolicyCover>()
            .HasOne(c => c.PolicyRisk)
            .WithMany(r => r.Covers)
            .HasForeignKey(c => c.PolicyRiskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PolicyCover>()
            .Property(c => c.SumInsured).HasPrecision(18, 2);
        builder.Entity<PolicyCover>()
            .Property(c => c.PremiumRate).HasPrecision(18, 4); // Plus de précision pour les taux
        builder.Entity<PolicyCover>()
            .Property(c => c.PremiumAmount).HasPrecision(18, 2);

        // Motor Quote Configurations
        builder.Entity<MotorQuote>()
            .HasOne(q => q.Client)
            .WithMany()
            .HasForeignKey(q => q.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorQuote>()
            .HasOne(q => q.Distributor)
            .WithMany()
            .HasForeignKey(q => q.DistributorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurer les relations avec les entités de véhicules pour éviter les cycles de cascade
        builder.Entity<MotorQuote>()
            .HasOne(q => q.VehicleCategory)
            .WithMany()
            .HasForeignKey(q => q.VehicleCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorQuote>()
            .HasOne(q => q.VehicleMake)
            .WithMany()
            .HasForeignKey(q => q.VehicleMakeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorQuote>()
            .HasOne(q => q.VehicleModel)
            .WithMany()
            .HasForeignKey(q => q.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorQuote>()
            .Property(q => q.VehicleValue).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.BasePremium).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.NetPremium).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.TotalPremium).HasPrecision(18, 2);
        
        // Ajouter les précisions pour les autres propriétés décimales de MotorQuote
        builder.Entity<MotorQuote>()
            .Property(q => q.SectionsPremium).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.Subtotal).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.ProfessionalDiscountPercent).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.CommercialDiscountPercent).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.TotalDiscount).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.NetPremiumBeforeShortTerm).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.ShortTermCoefficient).HasPrecision(18, 4);
        builder.Entity<MotorQuote>()
            .Property(q => q.TaxAmount).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.PolicyCostAmount).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.CommissionRate).HasPrecision(18, 2);
        builder.Entity<MotorQuote>()
            .Property(q => q.CommissionAmount).HasPrecision(18, 2);

        // Motor Quote Coverage
        builder.Entity<MotorQuoteCoverage>()
            .HasOne(qc => qc.MotorQuote)
            .WithMany(q => q.SelectedCoverages)
            .HasForeignKey(qc => qc.MotorQuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MotorQuoteCoverage>()
            .HasOne(qc => qc.MotorCoverage)
            .WithMany()
            .HasForeignKey(qc => qc.MotorCoverageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorQuoteCoverage>()
            .Property(qc => qc.PremiumAmount).HasPrecision(18, 2);

        // Ajouter les précisions pour PolicyCost
        builder.Entity<PolicyCost>()
            .Property(pc => pc.NetPremiumMin).HasPrecision(18, 2);
        builder.Entity<PolicyCost>()
            .Property(pc => pc.NetPremiumMax).HasPrecision(18, 2);
        builder.Entity<PolicyCost>()
            .Property(pc => pc.CostAmount).HasPrecision(18, 2);

        // Ajouter les précisions pour RatingFactor et ShortTermFactor
        builder.Entity<RatingFactor>()
            .Property(rf => rf.RatePercentage).HasPrecision(18, 4);

        builder.Entity<ShortTermFactor>()
            .Property(stf => stf.Coefficient).HasPrecision(18, 4);

        // Motor Product Coverage (Many-to-Many)
        builder.Entity<MotorProductCoverage>()
            .HasOne(pc => pc.MotorProduct)
            .WithMany(p => p.ProductCoverages)
            .HasForeignKey(pc => pc.MotorProductId);

        builder.Entity<MotorProductCoverage>()
            .HasOne(pc => pc.MotorCoverage)
            .WithMany(c => c.ProductCoverages)
            .HasForeignKey(pc => pc.MotorCoverageId);

        builder.Entity<MotorProductCoverage>()
            .Property(pc => pc.CustomPremium).HasPrecision(18, 2);

        // Motor Coverage
        builder.Entity<MotorCoverage>()
            .Property(c => c.FixedPremium).HasPrecision(18, 2);

        // Motor Policy Configurations
        builder.Entity<MotorPolicy>()
            .HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorPolicy>()
            .HasOne(p => p.Distributor)
            .WithMany()
            .HasForeignKey(p => p.DistributorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorPolicy>()
            .HasOne(p => p.MotorQuote)
            .WithMany()
            .HasForeignKey(p => p.MotorQuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorPolicy>()
            .Property(p => p.VehicleValue).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.BasePremium).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.NetPremium).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.TotalPremium).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.SectionsPremium).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.Subtotal).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.ProfessionalDiscountPercent).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.CommercialDiscountPercent).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.TotalDiscount).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.NetPremiumBeforeShortTerm).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.ShortTermCoefficient).HasPrecision(18, 4);
        builder.Entity<MotorPolicy>()
            .Property(p => p.TaxAmount).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.PolicyCostAmount).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.CommissionRate).HasPrecision(18, 2);
        builder.Entity<MotorPolicy>()
            .Property(p => p.CommissionAmount).HasPrecision(18, 2);

        // Motor Policy Coverage
        builder.Entity<MotorPolicyCoverage>()
            .HasOne(pc => pc.MotorPolicy)
            .WithMany(p => p.Coverages)
            .HasForeignKey(pc => pc.MotorPolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MotorPolicyCoverage>()
            .HasOne(pc => pc.MotorCoverage)
            .WithMany()
            .HasForeignKey(pc => pc.MotorCoverageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorPolicyCoverage>()
            .Property(pc => pc.PremiumAmount).HasPrecision(18, 2);

        // Motor Policy Endorsement
        builder.Entity<MotorPolicyEndorsement>()
            .HasOne(e => e.MotorPolicy)
            .WithMany(p => p.Endorsements)
            .HasForeignKey(e => e.MotorPolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MotorPolicyEndorsement>()
            .Property(e => e.PremiumAdjustment).HasPrecision(18, 2);
        builder.Entity<MotorPolicyEndorsement>()
            .Property(e => e.NewTotalPremium).HasPrecision(18, 2);

        // Motor Claim Configurations
        builder.Entity<MotorClaim>()
            .HasOne(c => c.MotorPolicy)
            .WithMany()
            .HasForeignKey(c => c.MotorPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorClaim>()
            .HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MotorClaim>()
            .Property(c => c.ClaimedAmount).HasPrecision(18, 2);
        builder.Entity<MotorClaim>()
            .Property(c => c.EstimatedAmount).HasPrecision(18, 2);
        builder.Entity<MotorClaim>()
            .Property(c => c.ApprovedAmount).HasPrecision(18, 2);
        builder.Entity<MotorClaim>()
            .Property(c => c.Deductible).HasPrecision(18, 2);
        builder.Entity<MotorClaim>()
            .Property(c => c.NetPayableAmount).HasPrecision(18, 2);

        builder.Entity<MotorClaim>()
            .HasIndex(c => c.ClaimNumber)
            .IsUnique();

        // Claim Document - now references base Claim class
        builder.Entity<ClaimDocument>()
            .HasOne(d => d.Claim)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Claim History - now references base Claim class
        builder.Entity<ClaimHistory>()
            .HasOne(h => h.Claim)
            .WithMany(c => c.History)
            .HasForeignKey(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Claim Third Party - still specific to MotorClaim
        builder.Entity<ClaimThirdParty>()
            .HasOne(t => t.MotorClaim)
            .WithMany(c => c.ThirdParties)
            .HasForeignKey(t => t.MotorClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ClaimThirdParty>()
            .Property(t => t.FaultPercentage).HasPrecision(5, 2);

        // ASACI Certificate Configuration
        builder.Entity<ASACICertificate>()
            .HasOne(c => c.Policy)
            .WithMany()
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ASACICertificate>()
            .HasIndex(c => c.CertificateNumber)
            .IsUnique();

        builder.Entity<ASACICertificate>()
            .HasIndex(c => c.VehicleRegistration);
    }
}
