using PISO.Contracts;

namespace PISO.Repository;

public class RepositoryManager : IRepositoryManager
{
    private readonly Lazy<IUserRepository> _user;
    private readonly Lazy<IBalanceRepository> _balance;
    private readonly Lazy<IDailyUsageRepository> _dailyUsage;
    private readonly Lazy<ISystemConfigRepository> _systemConfig;
    private readonly Lazy<IGlobalApiKeyRepository> _globalApiKey;

    public RepositoryManager(RepositoryContext context)
    {
        _user = new Lazy<IUserRepository>(() => new UserRepository(context));
        _balance = new Lazy<IBalanceRepository>(() => new BalanceRepository(context));
        _dailyUsage = new Lazy<IDailyUsageRepository>(() => new DailyUsageRepository(context));
        _systemConfig = new Lazy<ISystemConfigRepository>(() => new SystemConfigRepository(context));
        _globalApiKey = new Lazy<IGlobalApiKeyRepository>(() => new GlobalApiKeyRepository(context));
    }

    public IUserRepository User => _user.Value;
    public IBalanceRepository Balance => _balance.Value;
    public IDailyUsageRepository DailyUsage => _dailyUsage.Value;
    public ISystemConfigRepository SystemConfig => _systemConfig.Value;
    public IGlobalApiKeyRepository GlobalApiKey => _globalApiKey.Value;
}
