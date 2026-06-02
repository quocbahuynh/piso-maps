namespace PISO.Contracts;

public interface IRepositoryBase<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string documentId);
    Task AddAsync(string documentId, T entity);
    Task UpdateAsync(string documentId, T entity);
    Task DeleteAsync(string documentId);
}
