using PISO.Contracts;
using PISO.Entities.Models;

namespace PISO.Repository;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(RepositoryContext context)
        : base(context.Users)
    {
    }
}
