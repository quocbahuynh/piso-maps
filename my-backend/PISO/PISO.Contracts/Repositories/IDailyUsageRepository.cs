namespace PISO.Contracts;

public interface IDailyUsageRepository
{
    System.Threading.Tasks.Task UpdateDailyUsageAsync(string userId, PISO.Entities.Models.DailyUsage usage);
    System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<PISO.Entities.Models.DailyUsage>> GetDailyUsagesAsync(string userId, int days = 7);
}
