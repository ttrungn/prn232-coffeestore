namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public class ProductInMenu : BaseEntity<Guid>
{
    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }
    public Guid MenuId { get; set; }
    public virtual Menu? Menu { get; set; }
    public int Quantity { get; set; }
}