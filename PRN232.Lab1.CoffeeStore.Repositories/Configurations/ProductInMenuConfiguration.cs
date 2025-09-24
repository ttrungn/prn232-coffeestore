using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.Lab1.CoffeeStore.Repositories.Extensions;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Repositories.Configurations;

public class ProductInMenuConfiguration : IEntityTypeConfiguration<ProductInMenu>
{
    public void Configure(EntityTypeBuilder<ProductInMenu> builder)
    {
        builder.ToTable("ProductInMenus");

        builder.HasKey(pim => pim.Id);

        builder.Property(pim => pim.Id)
            .IsRequired();

        builder.Property(pim => pim.ProductId)
            .IsRequired();

        builder.Property(pim => pim.MenuId)
            .IsRequired();

        builder.Property(pim => pim.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(pim => pim.Product)
            .WithMany()
            .HasForeignKey(pim => pim.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pim => pim.Menu)
            .WithMany(m => m.ProductInMenus)
            .HasForeignKey(pim => pim.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pim => pim.MenuId);
        builder.HasIndex(pim => new { pim.ProductId, pim.MenuId })
            .IsUnique();
    }
}
