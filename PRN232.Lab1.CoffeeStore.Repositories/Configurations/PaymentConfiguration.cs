using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.Lab1.CoffeeStore.Repositories.Extensions;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Repositories.Enums;

namespace PRN232.Lab1.CoffeeStore.Repositories.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        // Configure PaymentMethod enum to be stored as integer
        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<int>();

        // Configure auditable properties using extension method
        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.OrderId)
            .IsUnique();
        
        builder.HasIndex(p => p.PaymentDate);
    }
}
