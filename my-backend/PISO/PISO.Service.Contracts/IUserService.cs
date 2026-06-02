using PISO.Shared.DataTransferObjects;

namespace PISO.Service.Contracts;

public interface IUserService
{
    Task<UserDto> SignUpOrSignInUserAsync(string userId, string email);
    Task<UserDto?> GetUserProfileAsync(string userId);
    Task<UserDto> RegenerateApiKeyAsync(string userId);
    Task UpdateBalanceAsync(string userId, long remainingCredits);
}
