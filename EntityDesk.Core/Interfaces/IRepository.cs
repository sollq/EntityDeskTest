namespace EntityDesk.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IList<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task MergeAsync(T entity);
    Task DeleteAsync(T entity);
}