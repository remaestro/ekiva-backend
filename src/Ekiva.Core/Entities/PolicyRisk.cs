using System;
using System.Collections.Generic;

namespace Ekiva.Core.Entities
{
    public class PolicyRisk
    {
        public Guid Id { get; set; }
        
        public Guid PolicyId { get; set; }
        public Policy Policy { get; set; } = null!;

        // Identification Véhicule
        public string RegistrationNumber { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        
        public Guid? VehicleMakeId { get; set; }
        public VehicleMake? VehicleMake { get; set; }
        
        public Guid? VehicleModelId { get; set; }
        public VehicleModel? VehicleModel { get; set; }
        
        public Guid VehicleCategoryId { get; set; }
        public VehicleCategory VehicleCategory { get; set; } = null!;

        // Caractéristiques techniques & Usage
        public int ManufactureYear { get; set; }
        public int FiscalPower { get; set; } // CV fiscaux
        public int NumberOfSeats { get; set; }
        public decimal VehicleValue { get; set; } // Valeur à neuf ou vénale
        
        public string Usage { get; set; } = "Private"; // Private, Commercial, Transport...

        // Primes spécifiques à ce risque
        public decimal NetPremium { get; set; }
        
        // Relations
        public ICollection<PolicyCover> Covers { get; set; } = new List<PolicyCover>();
    }
}
