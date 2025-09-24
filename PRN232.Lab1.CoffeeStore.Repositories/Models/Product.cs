namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public class Product : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Description { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}