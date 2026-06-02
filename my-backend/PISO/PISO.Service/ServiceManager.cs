using AutoMapper;
using PISO.Contracts;
using PISO.Service.Contracts;

namespace PISO.Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<ISystemConfigService> _systemConfig;
    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<IPlaceService> _placeService;
    private readonly Lazy<IUsageService> _usageService;

    public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, IRedisManager redis, IGoogleMapManager googleMap)
    {
        _systemConfig = new Lazy<ISystemConfigService>(() => new SystemConfigService(repositoryManager, logger, mapper, redis));
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager, logger, mapper, redis));
        _placeService = new Lazy<IPlaceService>(() => new PlaceService(googleMap));
        _usageService = new Lazy<IUsageService>(() => new UsageService(repositoryManager, redis, logger));
    }

    public ISystemConfigService SystemConfig => _systemConfig.Value;
    public IUserService User => _userService.Value;
    public IPlaceService Place => _placeService.Value;
    public IUsageService Usage => _usageService.Value;
}
