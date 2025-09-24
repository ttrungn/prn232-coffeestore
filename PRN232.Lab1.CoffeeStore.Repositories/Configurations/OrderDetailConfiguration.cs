using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.Lab1.CoffeeStore.Repositories.Extensions;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Repositories.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("OrderDetails");

        builder.HasKey(od => od.Id);

        builder.Property(od => od.Id)
            .IsRequired();

        builder.Property(od => od.OrderId)
            .IsRequired();

        builder.Property(od => od.ProductId)
            .IsRequired();

        builder.Property(od => od.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(od => od.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Configure auditable properties using extension method
        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(od => od.Product)
            .WithMany()
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(od => od.OrderId);
        builder.HasIndex(od => od.ProductId);
    }
}
