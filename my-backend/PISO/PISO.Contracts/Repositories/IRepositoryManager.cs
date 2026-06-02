namespace PISO.Contracts;

public interface IRepositoryManager
{
    IUserRepository User { get; }
    IBalanceRepository Balance { get; }
    IDailyUsageRepository DailyUsage { get; }
    ISystemConfigRepository SystemConfig { get; }
    IGlobalApiKeyRepository GlobalApiKey { get; }
}
