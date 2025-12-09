namespace Ekiva.Application.DTOs;

public class VehicleMakeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<VehicleModelDto> Models { get; set; } = new();
}

public class VehicleModelDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid MakeId { get; set; }
    public string MakeName { get; set; } = string.Empty;
}

public class VehicleCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
