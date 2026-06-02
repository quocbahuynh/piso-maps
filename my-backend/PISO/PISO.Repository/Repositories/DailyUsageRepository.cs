using PISO.Contracts;

namespace PISO.Repository;

public class DailyUsageRepository : IDailyUsageRepository
{
    private readonly RepositoryContext _context;

    public DailyUsageRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task UpdateDailyUsageAsync(string userId, PISO.Entities.Models.DailyUsage usage)
    {
        await _context.DailyUsage(userId).Document(usage.Date).SetAsync(usage);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<PISO.Entities.Models.DailyUsage>> GetDailyUsagesAsync(string userId, int days = 7)
    {
        var snapshot = await _context.DailyUsage(userId)
            .OrderByDescending("Date")
            .Limit(days)
            .GetSnapshotAsync();

        var usages = new System.Collections.Generic.List<PISO.Entities.Models.DailyUsage>();
        foreach (var doc in snapshot.Documents)
        {
            if (doc.Exists) usages.Add(doc.ConvertTo<PISO.Entities.Models.DailyUsage>());
        }
        return usages;
    }
}
