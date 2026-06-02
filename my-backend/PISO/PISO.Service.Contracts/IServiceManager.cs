namespace PISO.Service.Contracts;

public interface IServiceManager
{
    ISystemConfigService SystemConfig { get; }
    IUserService User { get; }
    IPlaceService Place { get; }
    IUsageService Usage { get; }
}
