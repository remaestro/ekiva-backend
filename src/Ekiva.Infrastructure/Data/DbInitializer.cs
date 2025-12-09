using Ekiva.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Ekiva.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<EkivaDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        string[] roleNames = { "Admin", "Broker", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Test Users
        await SeedTestUsersAsync(userManager);

        // Seed Reference Data
        await SeedReferenceDataAsync(context);
        
        // Seed Motor Insurance Data
        await SeedMotorInsuranceDataAsync(context);
    }

    private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // 1. Admin User
        var adminEmail = "admin@ekiva.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "System",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 2. Broker User
        var brokerEmail = "broker@ekiva.com";
        var brokerUser = await userManager.FindByEmailAsync(brokerEmail);
        if (brokerUser == null)
        {
            brokerUser = new ApplicationUser
            {
                UserName = brokerEmail,
                Email = brokerEmail,
                EmailConfirmed = true,
                FirstName = "Jean",
                LastName = "Kouassi",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(brokerUser, "Broker@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(brokerUser, "Broker");
            }
        }

        // 3. Regular User
        var userEmail = "user@ekiva.com";
        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "Marie",
                LastName = "Diallo",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(regularUser, "User@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        // 4. Agent User
        var agentEmail = "agent@ekiva.com";
        var agentUser = await userManager.FindByEmailAsync(agentEmail);
        if (agentUser == null)
        {
            agentUser = new ApplicationUser
            {
                UserName = agentEmail,
                Email = agentEmail,
                EmailConfirmed = true,
                FirstName = "Koffi",
                LastName = "Mensah",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(agentUser, "Agent@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agentUser, "User");
            }
        }
    }

    private static async Task SeedReferenceDataAsync(EkivaDbContext context)
    {
        // 1. Currencies
        if (!context.Currencies.Any())
        {
            context.Currencies.AddRange(
                new Currency { Code = "XOF", Name = "Franc CFA (UEMOA)", Symbol = "FCFA" },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€" },
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$" }
            );
            await context.SaveChangesAsync();
        }

        // 2. Vehicle Categories
        if (!context.VehicleCategories.Any())
        {
            context.VehicleCategories.AddRange(
                new VehicleCategory { Code = "TOURISME", Name = "Promenade et Affaires", Description = "Véhicules personnels" },
                new VehicleCategory { Code = "TRANSPORT_PERS", Name = "Transport de Personnel", Description = "Transport pour compte propre" },
                new VehicleCategory { Code = "TRANSPORT_PUB", Name = "Transport Public de Voyageurs", Description = "Taxis, Cars" },
                new VehicleCategory { Code = "MARCHANDISES", Name = "Transport de Marchandises", Description = "Camions, Camionnettes" },
                new VehicleCategory { Code = "DEUX_ROUES", Name = "Deux Roues", Description = "Motos, Scooters" }
            );
            await context.SaveChangesAsync();
        }

        // 3. Vehicle Makes & Models
        if (!context.VehicleMakes.Any())
        {
            var toyota = new VehicleMake { Code = "TOY", Name = "Toyota" };
            var hyundai = new VehicleMake { Code = "HYU", Name = "Hyundai" };
            var peugeot = new VehicleMake { Code = "PEU", Name = "Peugeot" };
            var mercedes = new VehicleMake { Code = "MER", Name = "Mercedes-Benz" };

            context.VehicleMakes.AddRange(toyota, hyundai, peugeot, mercedes);
            await context.SaveChangesAsync();

            context.VehicleModels.AddRange(
                new VehicleModel { Code = "COR", Name = "Corolla", MakeId = toyota.Id },
                new VehicleModel { Code = "RAV", Name = "RAV4", MakeId = toyota.Id },
                new VehicleModel { Code = "HIL", Name = "Hilux", MakeId = toyota.Id },
                new VehicleModel { Code = "TUC", Name = "Tucson", MakeId = hyundai.Id },
                new VehicleModel { Code = "SFE", Name = "Santa Fe", MakeId = hyundai.Id },
                new VehicleModel { Code = "3008", Name = "3008", MakeId = peugeot.Id },
                new VehicleModel { Code = "208", Name = "208", MakeId = peugeot.Id },
                new VehicleModel { Code = "CLE", Name = "Classe E", MakeId = mercedes.Id }
            );
            await context.SaveChangesAsync();
        }

        // 4. Professional Categories
        if (!context.ProfessionalCategories.Any())
        {
            context.ProfessionalCategories.AddRange(
                new ProfessionalCategory { Code = "SANS", Name = "Sans réduction", DiscountPercentage = 0m },
                new ProfessionalCategory { Code = "MEDECIN", Name = "Médecin / Pharmacien", DiscountPercentage = 0.20m },
                new ProfessionalCategory { Code = "AVOCAT", Name = "Avocat / Notaire", DiscountPercentage = 0.15m },
                new ProfessionalCategory { Code = "FONCTIONNAIRE", Name = "Fonctionnaire", DiscountPercentage = 0.10m }
            );
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task SeedMotorInsuranceDataAsync(EkivaDbContext context)
    {
        // 1. Rating Factors (Facteurs de Tarification)
        if (!context.RatingFactors.Any())
        {
            context.RatingFactors.AddRange(
                // Essence
                new RatingFactor { HorsepowerMin = 4, HorsepowerMax = 7, FuelType = "Essence", RatePercentage = 2.50m },
                new RatingFactor { HorsepowerMin = 8, HorsepowerMax = 9, FuelType = "Essence", RatePercentage = 3.00m },
                new RatingFactor { HorsepowerMin = 10, HorsepowerMax = 11, FuelType = "Essence", RatePercentage = 3.50m },
                new RatingFactor { HorsepowerMin = 12, HorsepowerMax = 14, FuelType = "Essence", RatePercentage = 4.00m },
                new RatingFactor { HorsepowerMin = 15, HorsepowerMax = 20, FuelType = "Essence", RatePercentage = 5.00m },
                new RatingFactor { HorsepowerMin = 21, HorsepowerMax = 999, FuelType = "Essence", RatePercentage = 6.00m },
                // Diesel
                new RatingFactor { HorsepowerMin = 4, HorsepowerMax = 7, FuelType = "Diesel", RatePercentage = 2.50m },
                new RatingFactor { HorsepowerMin = 8, HorsepowerMax = 9, FuelType = "Diesel", RatePercentage = 3.00m },
                new RatingFactor { HorsepowerMin = 10, HorsepowerMax = 11, FuelType = "Diesel", RatePercentage = 3.50m },
                new RatingFactor { HorsepowerMin = 12, HorsepowerMax = 14, FuelType = "Diesel", RatePercentage = 4.00m },
                new RatingFactor { HorsepowerMin = 15, HorsepowerMax = 20, FuelType = "Diesel", RatePercentage = 5.00m },
                new RatingFactor { HorsepowerMin = 21, HorsepowerMax = 999, FuelType = "Diesel", RatePercentage = 6.00m }
            );
            await context.SaveChangesAsync();
        }

        // 2. Short Term Factors (Coefficients Court Terme)
        if (!context.ShortTermFactors.Any())
        {
            context.ShortTermFactors.AddRange(
                new ShortTermFactor { Months = 1, Coefficient = 0.25m },
                new ShortTermFactor { Months = 3, Coefficient = 0.40m },
                new ShortTermFactor { Months = 6, Coefficient = 0.70m },
                new ShortTermFactor { Months = 9, Coefficient = 0.85m },
                new ShortTermFactor { Months = 12, Coefficient = 1.00m }
            );
            await context.SaveChangesAsync();
        }

        // 3. Policy Costs (Frais Accessoires)
        if (!context.PolicyCosts.Any())
        {
            context.PolicyCosts.AddRange(
                new PolicyCost { NetPremiumMin = 0, NetPremiumMax = 25000, CostAmount = 1000, ProductCode = "MOTOR" },
                new PolicyCost { NetPremiumMin = 25001, NetPremiumMax = 50000, CostAmount = 1500, ProductCode = "MOTOR" },
                new PolicyCost { NetPremiumMin = 50001, NetPremiumMax = 75000, CostAmount = 2000, ProductCode = "MOTOR" },
                new PolicyCost { NetPremiumMin = 75001, NetPremiumMax = 100000, CostAmount = 2500, ProductCode = "MOTOR" },
                new PolicyCost { NetPremiumMin = 100001, NetPremiumMax = null, CostAmount = 3000, ProductCode = "MOTOR" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Motor Coverages (Garanties/Sections)
        if (!context.MotorCoverages.Any())
        {
            context.MotorCoverages.AddRange(
                new MotorCoverage { CoverageCode = "SECTION_A", CoverageName = "Responsabilité Civile", SectionLetter = "A", FixedPremium = 0, IsMandatory = true, Description = "Dommages causés aux tiers" },
                new MotorCoverage { CoverageCode = "SECTION_B", CoverageName = "Défense et Recours", SectionLetter = "B", FixedPremium = 5000, IsMandatory = false, Description = "Frais de défense juridique" },
                new MotorCoverage { CoverageCode = "SECTION_C", CoverageName = "Incendie", SectionLetter = "C", FixedPremium = 0, IsMandatory = false, Description = "Dommages par incendie" },
                new MotorCoverage { CoverageCode = "SECTION_D", CoverageName = "Vol", SectionLetter = "D", FixedPremium = 0, IsMandatory = false, Description = "Vol du véhicule" },
                new MotorCoverage { CoverageCode = "SECTION_E", CoverageName = "Bris de Glace", SectionLetter = "E", FixedPremium = 5000, IsMandatory = false, Description = "Vitres et pare-brise" },
                new MotorCoverage { CoverageCode = "SECTION_F", CoverageName = "Dommages Collision", SectionLetter = "F", FixedPremium = 0, IsMandatory = false, Description = "Dommages au véhicule assuré" },
                new MotorCoverage { CoverageCode = "SECTION_G", CoverageName = "Catastrophes Naturelles", SectionLetter = "G", FixedPremium = 3000, IsMandatory = false, Description = "Tempête, inondation" },
                new MotorCoverage { CoverageCode = "SECTION_H", CoverageName = "Individuelle Conducteur", SectionLetter = "H", FixedPremium = 8000, IsMandatory = false, Description = "Dommages corporels du conducteur" }
            );
            await context.SaveChangesAsync();
        }

        // 5. Motor Products (Produits d'Assurance)
        if (!context.MotorProducts.Any())
        {
            var productTPO = new MotorProduct 
            { 
                ProductCode = "MOTOR_TPO", 
                ProductName = "Au Tiers (Third Party Only)", 
                Description = "Assurance responsabilité civile uniquement",
                IsActive = true
            };

            var productTPFT = new MotorProduct 
            { 
                ProductCode = "MOTOR_TPFT", 
                ProductName = "Tiers + Vol & Incendie", 
                Description = "RC + Vol + Incendie",
                IsActive = true
            };

            var productComp = new MotorProduct 
            { 
                ProductCode = "MOTOR_COMP", 
                ProductName = "Tous Risques (Comprehensive)", 
                Description = "Toutes garanties incluses",
                IsActive = true
            };

            context.MotorProducts.AddRange(productTPO, productTPFT, productComp);
            await context.SaveChangesAsync();

            // 6. Link Products with Coverages
            var coverageRC = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_A");
            var coverageDefense = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_B");
            var coverageIncendie = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_C");
            var coverageVol = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_D");
            var coverageBrisGlace = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_E");
            var coverageCollision = context.MotorCoverages.First(c => c.CoverageCode == "SECTION_F");

            // TPO: Only RC + Defense
            context.MotorProductCoverages.AddRange(
                new MotorProductCoverage { MotorProductId = productTPO.Id, MotorCoverageId = coverageRC.Id, IsIncludedByDefault = true },
                new MotorProductCoverage { MotorProductId = productTPO.Id, MotorCoverageId = coverageDefense.Id, IsIncludedByDefault = true }
            );

            // TPFT: RC + Defense + Vol + Incendie
            context.MotorProductCoverages.AddRange(
                new MotorProductCoverage { MotorProductId = productTPFT.Id, MotorCoverageId = coverageRC.Id, IsIncludedByDefault = true },
                new MotorProductCoverage { MotorProductId = productTPFT.Id, MotorCoverageId = coverageDefense.Id, IsIncludedByDefault = true },
                new MotorProductCoverage { MotorProductId = productTPFT.Id, MotorCoverageId = coverageIncendie.Id, IsIncludedByDefault = true },
                new MotorProductCoverage { MotorProductId = productTPFT.Id, MotorCoverageId = coverageVol.Id, IsIncludedByDefault = true }
            );

            // Comprehensive: All coverages
            var allCoverages = context.MotorCoverages.ToList();
            foreach (var coverage in allCoverages)
            {
                context.MotorProductCoverages.Add(
                    new MotorProductCoverage { MotorProductId = productComp.Id, MotorCoverageId = coverage.Id, IsIncludedByDefault = true }
                );
            }

            await context.SaveChangesAsync();
        }

        // 7. Tax Rates
        if (!context.ProductTaxRates.Any())
        {
            context.ProductTaxRates.AddRange(
                new ProductTaxRate { ProductType = ProductType.Motor, TaxName = "Taxes", Rate = 0.145m, IsFee = false },
                new ProductTaxRate { ProductType = ProductType.Motor, TaxName = "Frais de contrôle", Rate = 0.0125m, IsFee = true },
                new ProductTaxRate { ProductType = ProductType.Fire, TaxName = "Taxes", Rate = 0.25m, IsFee = false },
                new ProductTaxRate { ProductType = ProductType.Fire, TaxName = "Frais de contrôle", Rate = 0.0125m, IsFee = true },
                new ProductTaxRate { ProductType = ProductType.Liability, TaxName = "Taxes", Rate = 0.145m, IsFee = false },
                new ProductTaxRate { ProductType = ProductType.Liability, TaxName = "Frais de contrôle", Rate = 0.0125m, IsFee = true }
            );
            await context.SaveChangesAsync();
        }

        // 8. Commission Rates
        if (!context.CommissionRates.Any())
        {
            context.CommissionRates.AddRange(
                new CommissionRate { DistributorType = DistributorType.InternalAgent, ProductType = ProductType.Motor, Rate = 0.10m, Description = "Commission Agent Interne - Motor" },
                new CommissionRate { DistributorType = DistributorType.Broker, ProductType = ProductType.Motor, Rate = 0.125m, Description = "Commission Courtier - Motor" },
                new CommissionRate { DistributorType = DistributorType.GeneralAgent, ProductType = ProductType.Motor, Rate = 0.15m, Description = "Commission Agent Général - Motor" },
                new CommissionRate { DistributorType = DistributorType.Bancassurance, ProductType = ProductType.Motor, Rate = 0.08m, Description = "Commission Bancassurance - Motor" }
            );
            await context.SaveChangesAsync();
        }
    }
}
