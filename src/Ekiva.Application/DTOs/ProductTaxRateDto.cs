namespace Ekiva.Application.DTOs
{
    public class ProductTaxRateDto
    {
        public Guid Id { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
