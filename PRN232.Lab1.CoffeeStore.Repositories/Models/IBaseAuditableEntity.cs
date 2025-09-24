namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public interface IBaseAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    DateTime? DeletedAt { get; set; }
    bool IsActive { get; set; }
}
