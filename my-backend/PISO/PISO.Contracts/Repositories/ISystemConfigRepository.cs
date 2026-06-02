using PISO.Entities.Models;

namespace PISO.Contracts;

public interface ISystemConfigRepository
{
    Task<SystemConfig?> GetPlansAsync();
}
