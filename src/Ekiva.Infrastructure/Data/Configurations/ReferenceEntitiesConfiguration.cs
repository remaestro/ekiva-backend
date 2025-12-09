using Ekiva.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ekiva.Infrastructure.Data.Configurations;

public class CommissionRateConfiguration : IEntityTypeConfiguration<CommissionRate>
{
    public void Configure(EntityTypeBuilder<CommissionRate> builder)
    {
        builder.Property(p => p.Rate)
            .HasColumnType("decimal(18,4)"); // 4 décimales pour les taux (ex: 0.1250)
            
        builder.Property(p => p.Description)
            .HasMaxLength(200);
    }
}

public class ProductTaxRateConfiguration : IEntityTypeConfiguration<ProductTaxRate>
{
    public void Configure(EntityTypeBuilder<ProductTaxRate> builder)
    {
        builder.Property(p => p.Rate)
            .HasColumnType("decimal(18,4)");
            
        builder.Property(p => p.TaxName)
            .HasMaxLength(100)
            .IsRequired();
    }
}

public class ProfessionalCategoryConfiguration : IEntityTypeConfiguration<ProfessionalCategory>
{
    public void Configure(EntityTypeBuilder<ProfessionalCategory> builder)
    {
        builder.Property(p => p.DiscountPercentage)
            .HasColumnType("decimal(18,4)");
            
        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}

public class VehicleMakeConfiguration : IEntityTypeConfiguration<VehicleMake>
{
    public void Configure(EntityTypeBuilder<VehicleMake> builder)
    {
        builder.Property(v => v.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.Property(v => v.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        // Prevent cascade delete from VehicleMake to VehicleModel
        builder.HasOne(v => v.Make)
            .WithMany(m => m.Models)
            .HasForeignKey(v => v.MakeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VehicleCategoryConfiguration : IEntityTypeConfiguration<VehicleCategory>
{
    public void Configure(EntityTypeBuilder<VehicleCategory> builder)
    {
        builder.Property(v => v.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(v => v.Code)
            .HasMaxLength(50)
            .IsRequired();
    }
}

public class MotorPolicyConfiguration : IEntityTypeConfiguration<MotorPolicy>
{
    public void Configure(EntityTypeBuilder<MotorPolicy> builder)
    {
        // Prevent cascade delete from VehicleMake to MotorPolicy
        builder.HasOne(m => m.VehicleMake)
            .WithMany()
            .HasForeignKey(m => m.VehicleMakeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Prevent cascade delete from VehicleModel to MotorPolicy
        builder.HasOne(m => m.VehicleModel)
            .WithMany()
            .HasForeignKey(m => m.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
