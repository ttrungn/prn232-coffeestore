namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public class Menu : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public virtual ICollection<ProductInMenu> ProductInMenus { get; set; } = new List<ProductInMenu>();
}