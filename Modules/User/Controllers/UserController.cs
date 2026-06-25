using backend.Modules.User.DTOs;
using backend.Modules.User.Services;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.User.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
    private readonly IValidator<UpdateUserPreferencesDto> _updatePreferencesValidator;

    public UserController(
        IUserService userService,
        IValidator<UpdateProfileDto> updateProfileValidator,
        IValidator<UpdateUserPreferencesDto> updatePreferencesValidator)
    {
        _userService = userService;
        _updateProfileValidator = updateProfileValidator;
        _updatePreferencesValidator = updatePreferencesValidator;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var profile = await _userService.GetProfileAsync(User.GetRequiredUserId());
        return Ok(profile);
    }

    [Authorize]
    [HttpPatch("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        await _updateProfileValidator.ValidateAndThrowAsync(dto);
        var updated = await _userService.UpdateProfileAsync(User.GetRequiredUserId(), dto);
        return Ok(updated);
    }
    
    [Authorize]
    [HttpPatch("profile/photo")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfilePicture([FromForm] IFormFile file)
    {
        var updated = await _userService.UpdateProfilePictureAsync(User.GetRequiredUserId(), file);
        return Ok(updated);
    }

    [Authorize]
    [HttpGet("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> GetPreferences()
    {
        var preferences = await _userService.GetPreferencesAsync(User.GetRequiredUserId());
        return Ok(preferences);
    }

    [Authorize]
    [HttpPatch("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> UpdatePreferences([FromBody] UpdateUserPreferencesDto dto)
    {
        await _updatePreferencesValidator.ValidateAndThrowAsync(dto);
        var updatedPrefs = await _userService.UpdatePreferencesAsync(User.GetRequiredUserId(), dto);
        return Ok(updatedPrefs);
    }
}
