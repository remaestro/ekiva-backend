using System;
using System.Threading.Tasks;
using Ekiva.Application.DTOs.Motor;
using Ekiva.Core.Entities;

namespace Ekiva.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationDto request);
        Task<Policy> GetPolicyByIdAsync(Guid id);
        Task<Policy> ConvertToPolicyAsync(Guid quotationId); // Validation / Souscription
    }
}
