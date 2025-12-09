using System;

namespace Ekiva.Core.Interfaces
{
    public interface IRatingService
    {
        decimal CalculateCoverPremium(string coverCode, decimal vehicleValue, int fiscalPower, string usage);
    }
}
