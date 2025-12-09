using System;
using Ekiva.Core.Interfaces;

namespace Ekiva.Application.Services
{
    public class RatingService : IRatingService
    {
        public decimal CalculateCoverPremium(string coverCode, decimal vehicleValue, int fiscalPower, string usage)
        {
            // Logique de tarification simplifiée pour la démo
            // Dans un vrai système, cela viendrait d'une table de tarifs ou d'un moteur de règles
            
            decimal rate = 0;
            decimal basePremium = 0;

            switch (coverCode.ToUpper())
            {
                case "RC": // Responsabilité Civile
                    // Base sur la puissance fiscale
                    basePremium = 50000 + (fiscalPower * 2000);
                    if (usage == "Commercial") basePremium *= 1.5m;
                    return basePremium;

                case "THEFT": // Vol
                    rate = 0.015m; // 1.5% de la valeur
                    return vehicleValue * rate;

                case "FIRE": // Incendie
                    rate = 0.01m; // 1% de la valeur
                    return vehicleValue * rate;

                case "GLASS": // Bris de glace
                    return 25000; // Forfait

                case "DMG": // Dommages (Tous risques)
                    rate = 0.035m; // 3.5% de la valeur
                    return vehicleValue * rate;

                default:
                    return 0;
            }
        }
    }
}
