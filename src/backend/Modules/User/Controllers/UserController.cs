using backend.Modules.User.DTOs;
using backend.Modules.User.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.User.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // === PROFILE ===

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var profile = await _userService.GetProfileAsync();
        return Ok(profile);
    }

    [Authorize]
    [HttpPatch("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var updated = await _userService.UpdateProfileAsync(dto);
        return Ok(updated);
    }

    [Authorize]
    [HttpPatch("profile/photo")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfilePicture([FromForm] IFormFile file)
    {
        var updated = await _userService.UpdateProfilePictureAsync(file);
        return Ok(updated);
    }
    
    [Authorize]
    [HttpGet("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> GetPreferences()
    {
        var prefs = await _userService.GetPreferencesAsync();
        return Ok(prefs);
    }

    [Authorize]
    [HttpPatch("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> UpdatePreferences([FromBody] UpdateUserPreferencesDto dto)
    {
        var updatedPrefs = await _userService.UpdatePreferencesAsync(dto);
        return Ok(updatedPrefs);
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<UserProfileDto>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return Ok(user);
    }
}