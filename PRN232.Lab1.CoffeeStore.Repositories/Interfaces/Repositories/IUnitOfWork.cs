namespace PRN232.Lab1.CoffeeStore.Repositories.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T, TId> GetRepository<T, TId>()
        where T : class
        where TId : notnull;
    
    Task<int> SaveChangesAsync();
}