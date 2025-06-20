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
    [HttpPatch("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> UpdatePreferences([FromBody] UpdateUserPreferencesDto dto)
    {
        var updatedPrefs = await _userService.UpdatePreferencesAsync(dto);
        return Ok(updatedPrefs);
    }
}