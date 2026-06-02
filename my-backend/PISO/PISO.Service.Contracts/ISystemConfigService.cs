using PISO.Shared.DataTransferObjects;

namespace PISO.Service.Contracts;

public interface ISystemConfigService
{
    Task<PlansResponseDto?> GetPlansAsync();
}
