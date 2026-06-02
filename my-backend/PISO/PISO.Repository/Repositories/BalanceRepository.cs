using PISO.Contracts;
using PISO.Entities.Models;

namespace PISO.Repository;

public class BalanceRepository : IBalanceRepository
{
    private readonly RepositoryContext _context;

    public BalanceRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async Task CreateBalanceAsync(string userId, Balance balance)
    {
        await _context.Balances(userId).Document("current").SetAsync(balance);
    }

    public async Task<Balance?> GetBalanceAsync(string userId)
    {
        var snapshot = await _context.Balances(userId).Document("current").GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Balance>() : null;
    }

    public async Task DecrementBalanceAsync(string userId, int amount)
    {
        var docRef = _context.Balances(userId).Document("current");
        await docRef.UpdateAsync("RemainingCredits", Google.Cloud.Firestore.FieldValue.Increment(-amount));
    }
}
