using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PISO.Service.Contracts;
using PISO.Shared.DataTransferObjects;

namespace PISO.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UsersController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpPost("signup", Name = "SignUpOrSignIn")]
    public async Task<IActionResult> SignUpOrSignIn([FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] UserForSignUpDto? signUpDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = signUpDto?.Email;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID claim is missing in authentication token.");
        }

        if (string.IsNullOrEmpty(email))
        {
            email = string.Empty; // Fail-safe fallback if email is not available
        }

        var userDto = await _serviceManager.User.SignUpOrSignInUserAsync(userId, email);
        return Ok(userDto);
    }

    [HttpGet("profile", Name = "GetProfile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID claim is missing in authentication token.");
        }

        var userDto = await _serviceManager.User.GetUserProfileAsync(userId);
        if (userDto is null)
        {
            return NotFound($"User with ID {userId} was not found.");
        }
        return Ok(userDto);
    }

    [HttpPost("regenerate-key", Name = "RegenerateApiKey")]
    public async Task<IActionResult> RegenerateApiKey()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID claim is missing in authentication token.");
        }

        var userDto = await _serviceManager.User.RegenerateApiKeyAsync(userId);
        return Ok(userDto);
    }

    [HttpGet("logs", Name = "GetLogs")]
    public async Task<IActionResult> GetLogs()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID claim is missing in authentication token.");
        }

        var logs = await _serviceManager.Usage.GetUserLogsAsync(userId);
        return Ok(logs);
    }

    [HttpGet("daily-usage", Name = "GetDailyUsage")]
    public async Task<IActionResult> GetDailyUsage()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID claim is missing in authentication token.");
        }

        var dailyUsages = await _serviceManager.Usage.GetDailyUsagesAsync(userId, 7);
        return Ok(dailyUsages);
    }
}
