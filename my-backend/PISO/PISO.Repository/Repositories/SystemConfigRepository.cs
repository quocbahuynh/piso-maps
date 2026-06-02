using PISO.Contracts;
using PISO.Entities.Models;

namespace PISO.Repository;

public class SystemConfigRepository : RepositoryBase<SystemConfig>, ISystemConfigRepository
{
    public SystemConfigRepository(RepositoryContext context)
        : base(context.SystemConfigs)
    {
    }

    public async Task<SystemConfig?> GetPlansAsync()
    {
        return await GetByIdAsync("plans");
    }
}
