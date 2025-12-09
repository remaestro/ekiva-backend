using Ekiva.Application.DTOs.Motor;

namespace Ekiva.Application.Interfaces;

public interface IMotorQuoteService
{
    Task<MotorQuoteResponseDto> CreateQuoteAsync(CreateMotorQuoteDto dto);
    Task<MotorQuoteResponseDto> GetQuoteByIdAsync(Guid id);
    Task<List<MotorQuoteResponseDto>> GetQuotesByClientIdAsync(Guid clientId);
    Task<MotorQuoteResponseDto> AcceptQuoteAsync(Guid quoteId);
    Task<bool> RejectQuoteAsync(Guid quoteId);
}
