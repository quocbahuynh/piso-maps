using Google.Cloud.Firestore;
using PISO.Contracts;

namespace PISO.Repository;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected CollectionReference Collection { get; }

    protected RepositoryBase(CollectionReference collection)
    {
        Collection = collection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        var snapshot = await Collection.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }

    public async Task<T?> GetByIdAsync(string documentId)
    {
        var snapshot = await Collection.Document(documentId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
    }

    public async Task AddAsync(string documentId, T entity)
    {
        await Collection.Document(documentId).SetAsync(entity);
    }

    public async Task UpdateAsync(string documentId, T entity)
    {
        await Collection.Document(documentId).SetAsync(entity, SetOptions.MergeAll);
    }

    public async Task DeleteAsync(string documentId)
    {
        await Collection.Document(documentId).DeleteAsync();
    }
}
