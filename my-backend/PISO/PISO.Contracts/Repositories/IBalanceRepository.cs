using PISO.Entities.Models;

namespace PISO.Contracts;

public interface IBalanceRepository
{
    Task CreateBalanceAsync(string userId, Balance balance);
    Task<Balance?> GetBalanceAsync(string userId);
    Task DecrementBalanceAsync(string userId, int amount);
}
