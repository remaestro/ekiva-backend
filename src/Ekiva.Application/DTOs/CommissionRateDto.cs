namespace Ekiva.Application.DTOs
{
    public class CommissionRateDto
    {
        public Guid Id { get; set; }
        public string DistributorType { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
    }
}
