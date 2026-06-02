using PISO.Contracts;
using PISO.Entities.Models;

namespace PISO.Repository;

public class GlobalApiKeyRepository : RepositoryBase<GlobalApiKey>, IGlobalApiKeyRepository
{
    public GlobalApiKeyRepository(RepositoryContext context)
        : base(context.GlobalApiKeys)
    {
    }
}
