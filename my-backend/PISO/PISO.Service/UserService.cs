using System.Security.Cryptography;
using AutoMapper;
using Google.Cloud.Firestore;
using PISO.Contracts;
using PISO.Entities.Models;
using PISO.Service.Contracts;
using PISO.Shared;
using PISO.Shared.DataTransferObjects;

namespace PISO.Service;

internal sealed class UserService : IUserService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRedisManager _redis;

    public UserService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisManager redis)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _redis = redis;
    }

    public async Task<UserDto> SignUpOrSignInUserAsync(string userId, string email)
    {
        _logger.LogInfo($"Sign up or sign in request received for userId: {userId}");

        var user = await _repository.User.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogInfo($"User {userId} not found in Firestore. Commencing signup flow.");

            var systemConfig = await _repository.SystemConfig.GetPlansAsync();
            var planName = !string.IsNullOrEmpty(systemConfig?.Free?.Name)
                ? systemConfig.Free.Name
                : PisoConstants.Plans.Free;
            var maxCredits = systemConfig?.Free?.MaxCredits > 0
                ? systemConfig.Free.MaxCredits
                : PisoConstants.Credits.FreePlanMax;

            var apiKey = GenerateApiKey();
            user = new User
            {
                UserId = userId,
                Email = email,
                Plan = planName,
                MaxCredits = maxCredits,
                ApiKey = apiKey,
                Status = PisoConstants.UserStatus.Active,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            // 1. Add User record
            await _repository.User.AddAsync(userId, user);

            // 2. Add Global API Key record
            var globalKey = new GlobalApiKey
            {
                UserId = userId
            };
            await _repository.GlobalApiKey.AddAsync(apiKey, globalKey);

            // 3. Initialize Balance
            var balance = new Balance
            {
                RemainingCredits = maxCredits,
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            await _repository.Balance.CreateBalanceAsync(userId, balance);

            _logger.LogInfo($"Signup complete for user: {userId}. Generated API Key: {apiKey} under plan: {planName} with max credits: {maxCredits}");
        }
        else
        {
            _logger.LogInfo($"User {userId} found. Returning existing profile.");
            if (string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(email))
            {
                _logger.LogInfo($"User {userId} has empty email in DB. Updating email to: {email}");
                user.Email = email;
                await _repository.User.UpdateAsync(userId, user);
            }
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserProfileAsync(string userId)
    {
        var user = await _repository.User.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);

        var balance = await _repository.Balance.GetBalanceAsync(userId);
        if (balance is not null)
        {
            userDto.RemainingCredits = balance.RemainingCredits;
        }
        else
        {
            userDto.RemainingCredits = user.MaxCredits;
        }

        return userDto;
    }

    public async Task<UserDto> RegenerateApiKeyAsync(string userId)
    {
        _logger.LogInfo($"Regenerating API Key for userId: {userId}");

        var user = await _repository.User.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogError($"User {userId} not found in Firestore. Cannot regenerate API Key.");
            throw new Exception($"User with ID {userId} was not found.");
        }

        var oldApiKey = user.ApiKey;
        if (!string.IsNullOrEmpty(oldApiKey))
        {
            _logger.LogInfo($"Deleting old API Key mapping and cache for key: {oldApiKey}");
            await _repository.GlobalApiKey.DeleteAsync(oldApiKey);
            await _redis.RemoveAsync(RedisKeys.ApiKeyInfo(oldApiKey));
        }

        var newApiKey = GenerateApiKey();

        // 1. Add new API key mapping
        var globalKey = new GlobalApiKey
        {
            UserId = userId
        };
        await _repository.GlobalApiKey.AddAsync(newApiKey, globalKey);

        // 2. Update user profile with the new key
        user.ApiKey = newApiKey;
        await _repository.User.UpdateAsync(userId, user);

        _logger.LogInfo($"API Key successfully regenerated for user {userId}. New key: {newApiKey}");

        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateBalanceAsync(string userId, long remainingCredits)
    {
        _logger.LogInfo($"Syncing balance to Firestore for user: {userId}. Remaining credits: {remainingCredits}");
        var balance = new Balance
        {
            RemainingCredits = remainingCredits,
            UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        await _repository.Balance.CreateBalanceAsync(userId, balance);
    }

    private static string GenerateApiKey()
    {
        var bytes = new byte[16]; // 128 bits of entropy
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return PisoConstants.ApiKey.Prefix + Convert.ToHexString(bytes).ToLower();
    }
}
