using System;
using System.Collections.Generic;

namespace Ekiva.Application.DTOs.Motor
{
    public class CreateQuotationDto
    {
        public Guid ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid CurrencyId { get; set; }
        
        public List<RiskDto> Risks { get; set; } = new();
    }

    public class RiskDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public Guid VehicleCategoryId { get; set; }
        public int ManufactureYear { get; set; }
        public int FiscalPower { get; set; }
        public decimal VehicleValue { get; set; }
        public string Usage { get; set; } = "Private";
        
        public List<string> SelectedCoverCodes { get; set; } = new(); // RC, THEFT, etc.
    }
}
