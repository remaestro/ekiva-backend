using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Interfaces;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Services;

public class MotorQuoteService : IMotorQuoteService
{
    private readonly EkivaDbContext _context;
    private readonly IMotorPremiumCalculationService _premiumCalculationService;

    public MotorQuoteService(EkivaDbContext context, IMotorPremiumCalculationService premiumCalculationService)
    {
        _context = context;
        _premiumCalculationService = premiumCalculationService;
    }

    public async Task<MotorQuoteResponseDto> CreateQuoteAsync(CreateMotorQuoteDto dto)
    {
        // Calcul de la prime
        var calculationRequest = new MotorPremiumCalculationRequest
        {
            VehicleValue = dto.VehicleValue,
            Horsepower = dto.Horsepower,
            FuelType = dto.FuelType,
            DurationMonths = dto.DurationMonths,
            SelectedCoverageIds = dto.SelectedCoverageIds,
            ProfessionalDiscountPercent = dto.ProfessionalDiscountPercent,
            CommercialDiscountPercent = dto.CommercialDiscountPercent,
            CurrencyId = dto.CurrencyId,
            DistributorId = dto.DistributorId
        };

        var calculation = await _premiumCalculationService.CalculatePremiumAsync(calculationRequest);

        // Création du devis
        var quote = new MotorQuote
        {
            QuoteNumber = await GenerateQuoteNumberAsync(),
            QuoteDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            Status = QuoteStatus.Generated,
            
            ClientId = dto.ClientId,
            DistributorId = dto.DistributorId,
            MotorProductId = dto.MotorProductId,
            
            PolicyStartDate = dto.PolicyStartDate,
            PolicyEndDate = dto.PolicyEndDate,
            DurationMonths = dto.DurationMonths,
            
            VehicleCategoryId = dto.VehicleCategoryId,
            VehicleMakeId = dto.VehicleMakeId,
            VehicleModelId = dto.VehicleModelId,
            RegistrationNumber = dto.RegistrationNumber,
            ChassisNumber = dto.ChassisNumber,
            YearOfManufacture = dto.YearOfManufacture,
            Horsepower = dto.Horsepower,
            FuelType = dto.FuelType,
            VehicleValue = dto.VehicleValue,
            
            BasePremium = calculation.BasePremium,
            SectionsPremium = calculation.SectionsPremium,
            Subtotal = calculation.Subtotal,
            ProfessionalDiscountPercent = dto.ProfessionalDiscountPercent,
            CommercialDiscountPercent = dto.CommercialDiscountPercent,
            TotalDiscount = calculation.TotalDiscount,
            NetPremiumBeforeShortTerm = calculation.NetPremiumBeforeShortTerm,
            ShortTermCoefficient = calculation.ShortTermCoefficient,
            NetPremium = calculation.NetPremium,
            TaxAmount = calculation.TaxAmount,
            PolicyCostAmount = calculation.PolicyCostAmount,
            TotalPremium = calculation.TotalPremium,
            
            CommissionRate = calculation.CommissionRate,
            CommissionAmount = calculation.CommissionAmount,
            
            CurrencyId = dto.CurrencyId,
            Notes = dto.Notes
        };

        // Ajouter les garanties sélectionnées
        foreach (var coverageDetail in calculation.CoverageDetails)
        {
            quote.SelectedCoverages.Add(new MotorQuoteCoverage
            {
                MotorCoverageId = coverageDetail.CoverageId,
                IsSelected = true,
                PremiumAmount = coverageDetail.PremiumAmount
            });
        }

        _context.MotorQuotes.Add(quote);
        await _context.SaveChangesAsync();

        return await GetQuoteByIdAsync(quote.Id);
    }

    public async Task<MotorQuoteResponseDto> GetQuoteByIdAsync(Guid id)
    {
        var quote = await _context.MotorQuotes
            .Include(q => q.Client)
            .Include(q => q.Distributor)
            .Include(q => q.MotorProduct)
            .Include(q => q.VehicleCategory)
            .Include(q => q.VehicleMake)
            .Include(q => q.VehicleModel)
            .Include(q => q.Currency)
            .Include(q => q.SelectedCoverages)
                .ThenInclude(sc => sc.MotorCoverage)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            throw new Exception($"Quote with ID {id} not found");

        return MapToDto(quote);
    }

    public async Task<List<MotorQuoteResponseDto>> GetQuotesByClientIdAsync(Guid clientId)
    {
        var quotes = await _context.MotorQuotes
            .Include(q => q.Client)
            .Include(q => q.Distributor)
            .Include(q => q.MotorProduct)
            .Include(q => q.Currency)
            .Where(q => q.ClientId == clientId)
            .OrderByDescending(q => q.QuoteDate)
            .ToListAsync();

        return quotes.Select(MapToDto).ToList();
    }

    public async Task<MotorQuoteResponseDto> AcceptQuoteAsync(Guid quoteId)
    {
        var quote = await _context.MotorQuotes.FindAsync(quoteId);
        if (quote == null)
            throw new Exception($"Quote with ID {quoteId} not found");

        quote.Status = QuoteStatus.Accepted;
        await _context.SaveChangesAsync();

        return await GetQuoteByIdAsync(quoteId);
    }

    public async Task<bool> RejectQuoteAsync(Guid quoteId)
    {
        var quote = await _context.MotorQuotes.FindAsync(quoteId);
        if (quote == null)
            throw new Exception($"Quote with ID {quoteId} not found");

        quote.Status = QuoteStatus.Rejected;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<string> GenerateQuoteNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        var lastQuote = await _context.MotorQuotes
            .Where(q => q.QuoteDate.Year == year && q.QuoteDate.Month == month)
            .OrderByDescending(q => q.QuoteNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastQuote != null && !string.IsNullOrEmpty(lastQuote.QuoteNumber))
        {
            var parts = lastQuote.QuoteNumber.Split('-');
            if (parts.Length == 4 && int.TryParse(parts[3], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"QTE-{year}-{month:D2}-{sequence:D4}";
    }

    private MotorQuoteResponseDto MapToDto(MotorQuote quote)
    {
        return new MotorQuoteResponseDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            QuoteDate = quote.QuoteDate,
            ExpiryDate = quote.ExpiryDate,
            Status = quote.Status.ToString(),
            
            ClientName = quote.Client.FullName,
            DistributorName = quote.Distributor?.Name,
            ProductName = quote.MotorProduct.ProductName,
            
            RegistrationNumber = quote.RegistrationNumber,
            VehicleMake = quote.VehicleMake.Name,
            VehicleModel = quote.VehicleModel.Name,
            VehicleValue = quote.VehicleValue,
            
            NetPremium = quote.NetPremium,
            TaxAmount = quote.TaxAmount,
            PolicyCostAmount = quote.PolicyCostAmount,
            TotalPremium = quote.TotalPremium,
            
            CurrencyCode = quote.Currency.Code,
            
            CreatedAt = quote.CreatedAt
        };
    }
}
