namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public class BaseEntity<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;
}