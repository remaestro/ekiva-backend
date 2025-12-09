using System;
using System.Linq;
using System.Threading.Tasks;
using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Interfaces;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;

namespace Ekiva.Application.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRatingService _ratingService;

        public PolicyService(IPolicyRepository policyRepository, IRatingService ratingService)
        {
            _policyRepository = policyRepository;
            _ratingService = ratingService;
        }

        public async Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationDto request)
        {
            // 1. Création de l'entité Policy (Status Draft)
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IssueDate = DateTime.UtcNow,
                Status = "Draft",
                QuotationNumber = $"Q-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                CurrencyId = request.CurrencyId
            };

            decimal totalNetPremium = 0;

            // 2. Traitement des risques
            foreach (var riskDto in request.Risks)
            {
                var risk = new PolicyRisk
                {
                    Id = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    RegistrationNumber = riskDto.RegistrationNumber,
                    VehicleCategoryId = riskDto.VehicleCategoryId,
                    ManufactureYear = riskDto.ManufactureYear,
                    FiscalPower = riskDto.FiscalPower,
                    VehicleValue = riskDto.VehicleValue,
                    Usage = riskDto.Usage
                };

                decimal riskPremium = 0;

                // 3. Calcul des garanties
                foreach (var coverCode in riskDto.SelectedCoverCodes)
                {
                    var premium = _ratingService.CalculateCoverPremium(coverCode, risk.VehicleValue, risk.FiscalPower, risk.Usage);
                    
                    var cover = new PolicyCover
                    {
                        Id = Guid.NewGuid(),
                        PolicyRiskId = risk.Id,
                        CoverCode = coverCode,
                        CoverName = GetCoverName(coverCode),
                        SumInsured = coverCode == "RC" ? 0 : risk.VehicleValue, // Simplifié
                        PremiumAmount = premium,
                        PremiumRate = 0 // À calculer si besoin
                    };

                    risk.Covers.Add(cover);
                    riskPremium += premium;
                }

                risk.NetPremium = riskPremium;
                policy.Risks.Add(risk);
                totalNetPremium += riskPremium;
            }

            // 4. Totaux
            policy.TotalNetPremium = totalNetPremium;
            policy.TotalTaxes = totalNetPremium * 0.18m; // TVA 18% (Exemple)
            policy.TotalGrossPremium = policy.TotalNetPremium + policy.TotalTaxes;

            // 5. Sauvegarde via Repository
            await _policyRepository.AddAsync(policy);

            // 6. Mapping vers DTO
            return MapToDto(policy);
        }

        public async Task<Policy> GetPolicyByIdAsync(Guid id)
        {
            return await _policyRepository.GetPolicyWithDetailsAsync(id);
        }

        public async Task<Policy> ConvertToPolicyAsync(Guid quotationId)
        {
            var policy = await _policyRepository.GetByIdAsync(quotationId);
            if (policy == null) throw new Exception("Quotation not found");

            if (policy.Status != "Draft") throw new Exception("Policy is not in Draft status");

            policy.Status = "Active";
            policy.PolicyNumber = $"POL-{DateTime.UtcNow:yyyy}-{new Random().Next(10000, 99999)}";
            
            await _policyRepository.UpdateAsync(policy);
            return policy;
        }

        private string GetCoverName(string code)
        {
            return code switch
            {
                "RC" => "Responsabilité Civile",
                "THEFT" => "Vol",
                "FIRE" => "Incendie",
                "GLASS" => "Bris de Glace",
                "DMG" => "Dommages Tous Accidents",
                _ => code
            };
        }

        private QuotationResponseDto MapToDto(Policy policy)
        {
            return new QuotationResponseDto
            {
                PolicyId = policy.Id,
                QuotationNumber = policy.QuotationNumber ?? "",
                TotalPremium = policy.TotalGrossPremium,
                Status = policy.Status,
                Risks = policy.Risks.Select(r => new RiskResponseDto
                {
                    RegistrationNumber = r.RegistrationNumber,
                    NetPremium = r.NetPremium,
                    Covers = r.Covers.Select(c => new CoverResponseDto
                    {
                        CoverCode = c.CoverCode,
                        CoverName = c.CoverName,
                        PremiumAmount = c.PremiumAmount,
                        SumInsured = c.SumInsured
                    }).ToList()
                }).ToList()
            };
        }
    }
}
