using AutoMapper;
using PISO.Contracts;
using PISO.Service.Contracts;
using PISO.Shared;
using PISO.Shared.DataTransferObjects;

namespace PISO.Service;

internal sealed class SystemConfigService : ISystemConfigService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRedisManager _redis;

    public SystemConfigService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisManager redis)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _redis = redis;
    }

    public async Task<PlansResponseDto?> GetPlansAsync()
    {
        _logger.LogInfo("Fetching all plans from service layer");

        var cacheKey = RedisKeys.AllPlans;
        var cachedPlans = await _redis.GetAsync<PlansResponseDto>(cacheKey);
        if (cachedPlans is not null)
        {
            _logger.LogInfo("Fetched plans from Redis cache");
            return cachedPlans;
        }

        var plansEntity = await _repository.SystemConfig.GetPlansAsync();
        if (plansEntity is null)
            return null;

        var plansDto = _mapper.Map<PlansResponseDto>(plansEntity);

        await _redis.SetAsync(cacheKey, plansDto, TimeSpan.FromMinutes(10));
        _logger.LogInfo("Cached plans in Redis");

        return plansDto;
    }
}
